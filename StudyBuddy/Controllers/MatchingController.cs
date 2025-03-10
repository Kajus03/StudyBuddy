﻿using Microsoft.AspNetCore.Mvc;
using StudyBuddy.Attributes;
using StudyBuddy.Services.UserSessionService;
using StudyBuddy.Shared.Abstractions;
using StudyBuddy.Shared.DTOs;
using StudyBuddy.Shared.Models;
using StudyBuddy.Shared.ValueObjects;

namespace StudyBuddy.Controllers;

[CustomAuthorize]
[CustomBlockAuthorize]
public class MatchingController : Controller
{
    private readonly ILogger<MatchingController> _logger;
    private readonly IUserSessionService _userSessionService;
    private readonly IHttpClientFactory _clientFactory;

    public MatchingController(
        ILogger<MatchingController> logger,
        IUserSessionService userSessionService,
        IHttpClientFactory clientFactory)
    {
        _logger = logger;
        _userSessionService = userSessionService;
        _clientFactory = clientFactory;
    }

    public async Task<IActionResult> CurrentRandomUserProfile()
    {
        UserId currentUserId = (UserId)_userSessionService.GetCurrentUserId()!;

        HttpClient httpClient = _clientFactory.CreateClient("StudyBuddy.API");
        var responseCurrentUser = await httpClient.GetAsync($"api/v1/user/{currentUserId}");
        responseCurrentUser.EnsureSuccessStatusCode();

        User? currentUser = await responseCurrentUser.Content.ReadFromJsonAsync<User>();

        var responseUltimateUser =
            await httpClient.GetAsync($"api/v1/user/{currentUser.Id}/ultimate-seen-user");
        responseUltimateUser.EnsureSuccessStatusCode();

        User? currentRandomUser = await responseUltimateUser.Content.ReadFromJsonAsync<User>();

        ViewBag.ShowMatchRequestMessage = true;


        return View("RandomProfile", currentRandomUser);
    }

    public async Task<IActionResult> RandomProfile()
    {
        UserId currentUserId = (UserId)_userSessionService.GetCurrentUserId()!;

        ViewBag.CurrentUserId = currentUserId;
        ViewBag.ShowMatchRequestMessage = false;

        HttpClient httpClient = _clientFactory.CreateClient("StudyBuddy.API");

        var responseCurrentUser = await httpClient.GetAsync($"api/v1/user/{currentUserId}");
        responseCurrentUser.EnsureSuccessStatusCode();
        User? currentUser = await responseCurrentUser.Content.ReadFromJsonAsync<User>();

        // Prepare ViewBag for 'Go back!' button
        var responseViewedFirstProfile =
            await httpClient.GetAsync($"api/v1/user/{currentUser.Id}/not-seen-any-user");
        responseViewedFirstProfile.EnsureSuccessStatusCode();
        bool viewedFirstProfile = await responseViewedFirstProfile.Content.ReadFromJsonAsync<bool>();
        ViewBag.ViewedFirstProfile = viewedFirstProfile;

        var responseUsers = await httpClient.GetAsync("api/v1/user");
        responseUsers.EnsureSuccessStatusCode();
        List<User>? users = await responseUsers.Content.ReadFromJsonAsync<List<User>>();

        List<User> allUsers = users.Where(u => u.Id != currentUser.Id && (u.Flags & UserFlags.Admin) != UserFlags.Admin).ToList();

        bool useHobbiesFilter = currentUser.Hobbies != null && currentUser.Hobbies.Any();

        List<User> filteredUsers = allUsers.Where(user =>
            user.Traits.Subject == currentUser.Traits.Subject &&
            (!useHobbiesFilter || (user.Hobbies != null && user.Hobbies.Intersect(currentUser.Hobbies).Any()))
            ).ToList();

        List<IUser> unseenFilteredUsers = new();

        foreach (User user in filteredUsers)
        {
            var responseIsUserSeen =
                await httpClient.GetAsync($"api/v1/user/{currentUser.Id}/has-seen-user/{user.Id}");
            responseIsUserSeen.EnsureSuccessStatusCode();
            bool isUserSeen = await responseIsUserSeen.Content.ReadFromJsonAsync<bool>();

            if (!isUserSeen)
            {
                unseenFilteredUsers.Add(user);
            }
        }

        IUser? randomUser = null;

        if (unseenFilteredUsers.Any())
        {
            Random rnd = new();
            randomUser = unseenFilteredUsers[rnd.Next(unseenFilteredUsers.Count)];

            var responseUserSeen =
                await httpClient.PostAsync($"api/v1/user/{currentUser.Id}/user-seen/{randomUser.Id}", null);
            responseUserSeen.EnsureSuccessStatusCode();
        }

        TempData.Remove("HideGoBackButton");
        return View("RandomProfile", randomUser);
    }

    public IActionResult SetViewedFirstProfile()
    {
        ViewBag.ViewedFirstProfile = true;
        return RedirectToAction("RandomProfile");
    }

    public async Task<IActionResult> PenultimateProfile()
    {
        // Pass the current user's ID to the view
        UserId currentUserId = (UserId)_userSessionService.GetCurrentUserId()!;

        ViewBag.CurrentUserId = currentUserId;
        ViewBag.ShowMatchRequestMessage = false;

        HttpClient httpClient = _clientFactory.CreateClient("StudyBuddy.API");
        var responseCurrentUser = await httpClient.GetAsync($"api/v1/user/{currentUserId}");
        responseCurrentUser.EnsureSuccessStatusCode();

        User? currentUser = await responseCurrentUser.Content.ReadFromJsonAsync<User>();

        var responsePenultimateUser =
            await httpClient.GetAsync($"api/v1/user/{currentUser.Id}/penultimate-seen-user");
        responsePenultimateUser.EnsureSuccessStatusCode();

        User? penultimateUser = await responsePenultimateUser.Content.ReadFromJsonAsync<User>();

        TempData["HideGoBackButton"] = true;

        return View("RandomProfile", penultimateUser);
    }


    public async Task<IActionResult> UltimateProfile(bool disableGoBackButton = true)
    {
        // Pass the current user's ID to the view
        UserId currentUserId = (UserId)_userSessionService.GetCurrentUserId()!;

        ViewBag.CurrentUserId = currentUserId;
        ViewBag.ShowMatchRequestMessage = false;

        HttpClient httpClient = _clientFactory.CreateClient("StudyBuddy.API");
        var responseCurrentUser = await httpClient.GetAsync($"api/v1/user/{currentUserId}");
        responseCurrentUser.EnsureSuccessStatusCode();

        User? currentUser = await responseCurrentUser.Content.ReadFromJsonAsync<User>();

        var responseUltimateUser =
            await httpClient.GetAsync($"api/v1/user/{currentUser.Id}/ultimate-seen-user");
        responseUltimateUser.EnsureSuccessStatusCode();

        User? previousUser = await responseUltimateUser.Content.ReadFromJsonAsync<User>();

        if (disableGoBackButton)
        {
            TempData["HideGoBackButton"] = true;
        }
        else
        {
            TempData.Remove("HideGoBackButton");
        }
        return View("RandomProfile", previousUser);
    }


    [HttpPost]
    public async Task<IActionResult> MatchUsers(string currentUser, string otherUser, string redirectAction)
    {
        try
        {
            // Convert user IDs to the appropriate type (e.g., Guid)
            UserId currentUserId = UserId.From(Guid.Parse(currentUser));
            UserId otherUserId = UserId.From(Guid.Parse(otherUser));

            MatchDto matchDto = new() { CurrentUserId = currentUserId, OtherUserId = otherUserId };

            HttpClient httpClient = _clientFactory.CreateClient("StudyBuddy.API");
            var responseMatching =
                await httpClient.PostAsJsonAsync("api/v1/matching/match-users", matchDto);
            responseMatching.EnsureSuccessStatusCode();

            var responseIsMatched =
                await httpClient.GetAsync($"api/v1/matching/is-matched/{currentUserId}/{otherUserId}");
            responseIsMatched.EnsureSuccessStatusCode();

            bool isMatched = await responseIsMatched.Content.ReadFromJsonAsync<bool>();

            if (isMatched)
            {
                TempData["SuccessMessage"] = "Users matched successfully";
                ViewBag.ShowMatchRequestMessage = false;
            }
            else
            {
                // Store a "Match request sent" message in TempData
                TempData["MatchRequestSentMessage"] = "Match request sent.";
                ViewBag.ShowMatchRequestMessage = true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("An error occurred while matching user {currentUser} to {otherUser}", currentUser,
                otherUser);
            TempData["ErrorMessage"] = "An error occurred while matching users. " + ex.Message;
        }

        return redirectAction switch
        {
            "RandomProfile" => RedirectToAction("RandomProfile", "Matching"),
            "CurrentRandomUserProfile" => RedirectToAction("CurrentRandomUserProfile", "Matching"),
            "PenultimateProfile" => RedirectToAction("PenultimateProfile", "Matching"),
            _ => RedirectToAction("Index", "Home")
        };
    }
}
