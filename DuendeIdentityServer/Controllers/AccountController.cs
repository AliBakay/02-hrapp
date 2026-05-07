using DuendeIdentityServer.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DuendeIdentityServer.Controllers
{
    public class AccountController : Controller
    {
        [HttpGet]
        public IActionResult Login(string returnUrl)
        {
            var model = new LoginViewModel();
            model.ReturnUrl = returnUrl;
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            var testUser = Config.TestUsers.Where(x => x.Username == model.Username).FirstOrDefault();

            if (testUser != null)
            {
                var claims = testUser.Claims;
                var identity = new ClaimsIdentity(claims, "cookie");
                //await HttpContext.SignInAsync("cookie", new ClaimsPrincipal(identity));
                await HttpContext.SignInAsync("idsrv", new ClaimsPrincipal(identity));
                return Redirect(model.ReturnUrl);
            }

            ModelState.AddModelError("", "Invalid login");
            return View(model);
        }
    }
}
