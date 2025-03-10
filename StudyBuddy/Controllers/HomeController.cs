﻿using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using StudyBuddy.Attributes;
using StudyBuddy.Models;
using StudyBuddy.Services.UserSessionService;
using StudyBuddy.Shared.Abstractions;
using StudyBuddy.Shared.Models;

namespace StudyBuddy.Controllers;

[CustomBlockAuthorize]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IHttpClientFactory _clientFactory;
    private readonly IUserSessionService _userSessionService;

    public HomeController(ILogger<HomeController> logger, IHttpClientFactory clientFactory, IUserSessionService userSessionService)
    {
        _logger = logger;
        _clientFactory = clientFactory;
        _userSessionService = userSessionService;
    }

    public async Task<IActionResult> Index()
    {
        var httpClient = _clientFactory.CreateClient("StudyBuddy.API");
        var currentUserId = _userSessionService.GetCurrentUserId();
        if (currentUserId != null)
        {
            var responseUser = await httpClient.GetAsync($"api/v1/user/{currentUserId.Value}");

            if (responseUser.IsSuccessStatusCode)
            {
                var user = await responseUser.Content.ReadFromJsonAsync<User>();

                if (user != null && user.Flags.HasFlag(UserFlags.Admin))
                {
                    return RedirectToAction("Index", "Admin");
                }

                ViewBag.User = user;
            }
        }

        var responseTotalUsers = await httpClient.GetAsync("api/v1/utility/total-users");
        responseTotalUsers.EnsureSuccessStatusCode();
        int totalUsers = await responseTotalUsers.Content.ReadFromJsonAsync<int>();

        return View(totalUsers);
    }

    public IActionResult Privacy() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() =>
        View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
