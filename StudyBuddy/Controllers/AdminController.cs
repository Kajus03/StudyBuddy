using Microsoft.AspNetCore.Mvc;
using StudyBuddy.Attributes;
using StudyBuddy.Shared.Abstractions;
using StudyBuddy.Shared.DTOs;
using StudyBuddy.Shared.Models;
namespace StudyBuddy.Controllers
{
    public class AdminController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AdminController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [CustomAdminAuthorize]
        public async Task<IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient("StudyBuddy.API");

            // Fetch users
            var userResponse = await client.GetAsync($"/api/v1/user");
            if (!userResponse.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Failed to fetch users.";
                return View("Error");
            }
            var users = await userResponse.Content.ReadFromJsonAsync<List<User>>();

            // Fetch blocks
            var blockResponse = await client.GetAsync($"api/v1/admin");
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
                BlockedUntil = blocks.FirstOrDefault(b => b.BlockedUserId == user.Id)?.BlockedUntil
            }).ToList();

            return View("AdminDashboard", usersWithStatus);
        }

        public class UserWithBlockStatus
        {
            public User User { get; set; }
            public bool IsBlocked { get; set; }
            public DateTime? BlockedUntil { get; set; }
        }

    }
}
