using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using StudyBuddy.Services.UserSessionService;
using StudyBuddy.Shared.Models;
using StudyBuddy.Shared.ValueObjects;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;

namespace StudyBuddy.Attributes
{
    public class CustomBlockAuthorizeAttribute : TypeFilterAttribute
    {
        public CustomBlockAuthorizeAttribute() : base(typeof(CustomBlockAuthorizeFilter))
        {
        }

        private class CustomBlockAuthorizeFilter : IAsyncAuthorizationFilter
        {
            private readonly IUserSessionService _userSessionService;
            private readonly IHttpClientFactory _httpClientFactory;

            public CustomBlockAuthorizeFilter(IUserSessionService userSessionService, IHttpClientFactory httpClientFactory)
            {
                _userSessionService = userSessionService;
                _httpClientFactory = httpClientFactory;
            }

            public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
            {
                var userId = _userSessionService.GetCurrentUserId();

                if (userId != null)
                {
                    var (isBlocked, blockReason, blockTime) = await IsUserBlocked(userId.Value);
                    if (isBlocked)
                    {
                        context.HttpContext.Response.Cookies.Delete("UserId");
                        context.HttpContext.Response.Cookies.Delete("Username");

                        context.HttpContext.Items["BlockReason"] = blockReason;
                        context.HttpContext.Items["BlockTime"] = blockTime;
                        context.Result = new ViewResult
                        {
                            ViewName = "~/Views/Home/BlockedUserView.cshtml",
                            ViewData = new Microsoft.AspNetCore.Mvc.ViewFeatures.ViewDataDictionary(new Microsoft.AspNetCore.Mvc.ModelBinding.EmptyModelMetadataProvider(), new Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary())
                            {
                                ["BlockReason"] = blockReason,
                                ["BlockTime"] = blockTime
                            }
                        };
                    }
                }
            }

            private async Task<(bool, string?, DateTime?)> IsUserBlocked(UserId userId)
            {
                HttpClient httpClient = _httpClientFactory.CreateClient("StudyBuddy.API");
                var response = await httpClient.GetAsync("api/v1/admin/"); 

                if (!response.IsSuccessStatusCode)
                {
                    return (false, null, null);
                }

                var systemBlocks = await response.Content.ReadFromJsonAsync<List<SystemBlock>>();

                var block = systemBlocks?.FirstOrDefault(b => b.BlockedUserId == userId);

                if (block != null)
                {
                    return (true, block.SystemBlockReason?.Reason, block.BlockedUntil);
                }

                return (false, null, null);
            }
        }
    }
}
