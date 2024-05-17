using Microsoft.AspNetCore.Mvc;
using StudyBuddy.Attributes;
using StudyBuddy.Models;
using StudyBuddy.Services.UserSessionService;
using StudyBuddy.Shared.Abstractions;
using StudyBuddy.Shared.DTOs;
using StudyBuddy.Shared.DTOs.systemBlockDtos;
using StudyBuddy.Shared.Models;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace StudyBuddy.Controllers
{
    public class AdminController : Controller
    {
        private readonly ILogger<AdminController> _logger;
        private readonly IUserSessionService _userSessionService;
        private readonly IHttpClientFactory _httpClientFactory;

        public AdminController(
            ILogger<AdminController> logger,
            IUserSessionService userSessionService,
            IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _userSessionService = userSessionService;
            _httpClientFactory = httpClientFactory;
        }

        [CustomAdminAuthorize]
        public async Task<IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient("StudyBuddy.API");

            // Fetch users
            var userResponse = await client.GetAsync("/api/v1/user");
            if (!userResponse.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Failed to fetch users.";
                return View("Error");
            }
            var users = await userResponse.Content.ReadFromJsonAsync<List<User>>();

            // Fetch blocks
            var blockResponse = await client.GetAsync("/api/v1/admin");
            if (!blockResponse.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Failed to fetch system blocks.";
                return View("Error");
            }
            var blocks = await blockResponse.Content.ReadFromJsonAsync<List<SystemBlock>>();

            // Combine user data with block status
            var usersWithStatus = users
                .Where(user => (user.Flags & UserFlags.Admin) != UserFlags.Admin)
                .Select(user => new UserWithBlockStatus
                {
                    User = user,
                    IsBlocked = blocks.Any(b => b.BlockedUserId == user.Id),
                    BlockId = blocks.FirstOrDefault(b => b.BlockedUserId == user.Id)?.Id ?? Guid.Empty,
                    BlockedUntil = blocks.FirstOrDefault(b => b.BlockedUserId == user.Id)?.BlockedUntil ?? DateTime.MinValue
                }).ToList();

            return View("AdminDashboard", usersWithStatus);
        }

        public class UserWithBlockStatus
        {
            public User User { get; set; }
            public bool IsBlocked { get; set; }
            public Guid BlockId { get; set; }

            public DateTime? BlockedUntil { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> BlockUser(SystemBlockRequest blockRequest)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("StudyBuddy.API");

            var response = await httpClient.PostAsJsonAsync("/api/v1/admin", blockRequest);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to block user: {Content}", errorContent);
                TempData["ErrorMessage"] = "Failed to block user.";
                return RedirectToAction("Index");
            }

            _logger.LogInformation("User blocked successfully: {UserId}", blockRequest.BlockedUserId);
            TempData["SuccessMessage"] = "User blocked successfully";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> UnblockUser(Guid blockId)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("StudyBuddy.API");
            var response = await httpClient.DeleteAsync($"/api/v1/admin/{blockId}");

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to unblock user: {Content}", errorContent);
                TempData["ErrorMessage"] = "Failed to unblock user.";
                return RedirectToAction("Index");
            }

            _logger.LogInformation("User unblocked successfully: {BlockId}", blockId);
            TempData["SuccessMessage"] = "User unblocked successfully";
            return RedirectToAction("Index");
        }
    }
}
