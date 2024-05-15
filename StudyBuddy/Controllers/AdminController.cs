using Microsoft.AspNetCore.Mvc;
using StudyBuddy.Attributes;
namespace StudyBuddy.Controllers
{
    public class AdminController : Controller
    {
        [CustomAdminAuthorize]
        public IActionResult Index()
        {
            return View("AdminDashboard");
        }
    }
}
