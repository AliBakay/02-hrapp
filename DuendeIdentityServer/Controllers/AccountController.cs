using Duende.IdentityServer.Services;
using DuendeIdentityServer.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DuendeIdentityServer.Controllers
{
    public class AccountController : Controller
    {
        private readonly IIdentityServerInteractionService _interaction;

        public AccountController(IIdentityServerInteractionService interaction)
        {
            _interaction = interaction;
        }

        // ─── Login ────────────────────────────────────────────────────────────────

        [HttpGet]
        public IActionResult Login(string returnUrl)
        {
            var model = new LoginViewModel { ReturnUrl = returnUrl };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            var testUser = Config.TestUsers
                .FirstOrDefault(x => x.Username == model.Username
                                  && x.Password == model.Password);

            if (testUser is not null)
            {
                var claims = testUser.Claims.ToList();
                // Ensure SubjectId is included as the 'sub' claim
                if (!claims.Any(c => c.Type == "sub"))
                    claims.Add(new Claim("sub", testUser.SubjectId));

                var identity = new ClaimsIdentity(claims, "idsrv");
                await HttpContext.SignInAsync("idsrv", new ClaimsPrincipal(identity));
                return Redirect(model.ReturnUrl ?? "/");
            }

            ModelState.AddModelError("", "Invalid username or password.");
            return View(model);
        }

        // ─── Logout ───────────────────────────────────────────────────────────────

        /// <summary>
        /// Called by the client (HrApp) via the OIDC end-session endpoint.
        /// Signs the user out of the IDS session and redirects back to the client.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Logout(string logoutId)
        {
            // Get the logout context so we know where to redirect after logout
            var logoutContext = await _interaction.GetLogoutContextAsync(logoutId);

            // Sign out of the IdentityServer session
            await HttpContext.SignOutAsync("idsrv");

            // Redirect back to the client app (HrApp) or to home
            var postLogoutRedirectUri = logoutContext?.PostLogoutRedirectUri ?? "/";
            return Redirect(postLogoutRedirectUri);
        }
    }
}

