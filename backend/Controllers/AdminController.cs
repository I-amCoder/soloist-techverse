using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace AspnetCoreMvcFull.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        public IActionResult Users()
        {
            return View();
        }

        public IActionResult Items()
        {
            return View();
        }

        public IActionResult Claims()
        {
            return View();
        }

        public IActionResult Reports()
        {
            return View();
        }
    }
} 