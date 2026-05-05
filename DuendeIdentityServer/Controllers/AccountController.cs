using Microsoft.AspNetCore.Mvc;

namespace DuendeIdentityServer.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
