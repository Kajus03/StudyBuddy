using Markdig.Syntax;
using Microsoft.AspNetCore.Mvc;
using StudyBuddy.Attributes;
using StudyBuddy.Models;
using StudyBuddy.Services.UserSessionService;
using StudyBuddy.Shared.Abstractions;
using StudyBuddy.Shared.DTOs;
using StudyBuddy.Shared.Models;
using StudyBuddy.Shared.ValueObjects;

namespace StudyBuddy.Controllers.ProfileController;

public class ProfileController : Controller
{
    private readonly ILogger<ProfileController> _logger;
    private readonly IUserSessionService _userSessionService;
    private readonly IHttpClientFactory _clientFactory;

    public ProfileController(
        ILogger<ProfileController> logger,
        IUserSessionService userSessionService,
        IHttpClientFactory clientFactory
    )
    {
        _logger = logger;
        _userSessionService = userSessionService;
        _clientFactory = clientFactory;
    }

    public async Task<IActionResult> UserProfile(string id)
    {
        if (Guid.TryParse(id, out Guid parsedGuid))
        {
            UserId parseUserId = UserId.From(parsedGuid);

            HttpClient httpClient = _clientFactory.CreateClient("StudyBuddy.API");
            var responseUser = await httpClient.GetAsync($"api/v1/user/{parseUserId}");


            if (!responseUser.IsSuccessStatusCode)
            {
                ErrorDto? error = await responseUser.Content.ReadFromJsonAsync<ErrorDto>();
                TempData["ErrorMessage"] = error?.Message;
                return RedirectToAction("Index", "Home");
            }

            IUser? user = await responseUser.Content.ReadFromJsonAsync<User>();

            return View("ViewFullProfile", user);
        }

        ErrorViewModel errorModel = new() { ErrorMessage = "User not found or invalid ID." };
        return View("Error", errorModel);
    }

    public IActionResult CreateProfile()
    {
        // Don't let the user access this page if they are already logged in
        if (_userSessionService.GetCurrentUserId() != null)
        {
            return RedirectToAction("Index", "Home");
        }

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> SaveProfile(ProfileDto profileDto)
    {
        HttpClient httpClient = _clientFactory.CreateClient("StudyBuddy.API");

        var responseRegisterUser = await httpClient.PostAsJsonAsync("api/v1/user/register", profileDto);

        if (!responseRegisterUser.IsSuccessStatusCode)
        {
            ErrorDto? error = await responseRegisterUser.Content.ReadFromJsonAsync<ErrorDto>();
            TempData["ErrorMessage"] = error?.Message;
            return RedirectToAction("CreateProfile");
        }

        _logger.LogInformation("Profile created successfully for user: {UserName}", profileDto.Username);
        TempData["SuccessMessage"] = "Profile created successfully";
        return RedirectToAction("CreateProfile");
    }

    public async Task<IActionResult> Login(string? username, string? password)
    {
        // Authenticate the user
        if (username == null || password == null)
        {
            TempData["ErrorMessage"] = "Username and password are required";
            return View("Login");
        }

        bool isAuthenticated = await _userSessionService.AuthenticateUser(username, password);
        if (!isAuthenticated)
        {
            TempData["ErrorMessage"] = "Invalid username or password";
            return RedirectToAction("Login");
        }

        CookieOptions cookieOptions = new()
        {
            Expires = DateTime.Now.AddHours(1),
            HttpOnly = true,
            IsEssential = true,
            SameSite = SameSiteMode.Strict,
            Secure = true
        };
        Response.Cookies.Append("UserId", _userSessionService.GetCurrentUserId().ToString()!, cookieOptions);

        IUser? user = await _userSessionService.GetUser(username);
        if (user == null)
        {
            TempData["ErrorMessage"] = "User not found";
            return RedirectToAction("Login");
        }

        if (user.Flags.HasFlag(UserFlags.Admin))
        {
            return RedirectToAction("Index", "Admin");
        }
        else if (user.Flags.HasFlag(UserFlags.Registered))
        {
            return RedirectToAction("RandomProfile", "Matching");
        }
        else
        {
            TempData["ErrorMessage"] = "Access Denied";
            return RedirectToAction("Login");
        }
    }

    [CustomAuthorize]
    [HttpPost]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("UserId");
        Response.Cookies.Delete("Username");

        return RedirectToAction("Index", "Home");
    }

    [CustomAuthorize]
    public async Task<IActionResult> EditProfile()
    {
        UserId currentUserId = (UserId)_userSessionService.GetCurrentUserId()!;

        HttpClient httpClient = _clientFactory.CreateClient("StudyBuddy.API");
        var responseCurrentUser = await httpClient.GetAsync($"api/v1/user/{currentUserId}");
        responseCurrentUser.EnsureSuccessStatusCode();

        User? user = await responseCurrentUser.Content.ReadFromJsonAsync<User>();

        return View(user);
    }

    [CustomAuthorize]
    public async Task<IActionResult> UpdateProfile(UpdateUserDto updateUserDto)
    {
        UserId currentUserId = (UserId)_userSessionService.GetCurrentUserId()!;

        HttpClient httpClient = _clientFactory.CreateClient("StudyBuddy.API");

        var responseUpdateUser =
            await httpClient.PutAsJsonAsync($"api/v1/user/{currentUserId}/update", updateUserDto);

        if (!responseUpdateUser.IsSuccessStatusCode)
        {
            ErrorDto? error = await responseUpdateUser.Content.ReadFromJsonAsync<ErrorDto>();
            TempData["ErrorMessage"] = error?.Message;
            return RedirectToAction("EditProfile");
        }

        TempData["SuccessMessage"] = "Profile updated successfully";
        return RedirectToAction("EditProfile");
    }
}
