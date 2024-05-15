using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using StudyBuddy.Services.UserSessionService;
using StudyBuddy.Shared.Abstractions;
using StudyBuddy.Shared.Models;

namespace StudyBuddy.Attributes;

public class CustomAdminAuthorize : TypeFilterAttribute
{
    public CustomAdminAuthorize() : base(typeof(CustomAdminAuthorizeFilter))
    {
    }

    private class CustomAdminAuthorizeFilter : IAuthorizationFilter
    {
        private readonly IUserSessionService _userSessionService;

        public CustomAdminAuthorizeFilter(IUserSessionService userSessionService) =>
            _userSessionService = userSessionService;

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var username = _userSessionService.GetCurrentUsername();
            if (username == null)
            {
                context.Result = new RedirectToActionResult("Login", "Profile", null);
                return;
            }

            var user = _userSessionService.GetUser(username).Result;
            if (user == null || !user.Flags.HasFlag(UserFlags.Admin))
            {
                context.Result = new RedirectToActionResult("Index", "Home", null);
            }
        }
    }
}
