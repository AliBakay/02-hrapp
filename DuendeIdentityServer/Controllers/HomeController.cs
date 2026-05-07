using DuendeIdentityServer.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace DuendeIdentityServer.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            var duendeConfig = new DuendeConfigViewModel();
            return View(duendeConfig);
        }
    }
}
