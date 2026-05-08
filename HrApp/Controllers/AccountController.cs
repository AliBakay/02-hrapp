using HrApp.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HrApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public AccountController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // ─── Entry point: choose how to log in ───────────────────────────────────

        public IActionResult Login()
        {
            return View();
        }

        // ─── Local login – by Username ────────────────────────────────────────────

        [HttpGet]
        public IActionResult LoginUserName()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> LoginUserName(LoginUserNameViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByNameAsync(model.UserName);
                if (user is not null)
                {
                    var result = await _signInManager.PasswordSignInAsync(
                        user, model.Password, isPersistent: false, lockoutOnFailure: false);

                    if (result.Succeeded)
                        return RedirectToAction("Index", "Home");

                    ModelState.AddModelError("", "Invalid login attempt.");
                }
                else
                {
                    ModelState.AddModelError("", "Invalid login attempt.");
                }
            }
            return View(model);
        }

        // ─── Local login – by Email ───────────────────────────────────────────────

        [HttpGet]
        public IActionResult LoginEmail()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> LoginEmail(LoginEmailViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user is not null)
                {
                    var result = await _signInManager.PasswordSignInAsync(
                        user, model.Password, isPersistent: false, lockoutOnFailure: false);

                    if (result.Succeeded)
                        return RedirectToAction("Index", "Home");

                    ModelState.AddModelError("", "Invalid login attempt.");
                }
                else
                {
                    ModelState.AddModelError("", "Invalid login attempt.");
                }
            }
            return View(model);
        }

        // ─── External login – Duende IdentityServer (OIDC) ───────────────────────

        /// <summary>
        /// Redirects the user to Duende IdentityServer's login page via OIDC challenge.
        /// </summary>
        public IActionResult LoginExternalProvider()
        {
            // After Duende authenticates the user it will redirect back to
            // /Account/ExternalProviderResponse via signin-oidc middleware.
            string? redirectUrl = Url.Action(nameof(ExternalProviderResponse), "Account");
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(
                "oidc", redirectUrl);
            return new ChallengeResult("oidc", properties);
        }

        /// <summary>
        /// Callback invoked by ASP.NET Core after the OIDC middleware processes
        /// the token returned by Duende. Creates or links the local IdentityUser.
        /// </summary>
        public async Task<IActionResult> ExternalProviderResponse()
        {
            ExternalLoginInfo? externalLoginInfo =
                await _signInManager.GetExternalLoginInfoAsync();

            if (externalLoginInfo is null)
                return RedirectToAction(nameof(Login));

            // Try to sign in with the external login info directly
            var signInResult = await _signInManager.ExternalLoginSignInAsync(
                externalLoginInfo.LoginProvider,
                externalLoginInfo.ProviderKey,
                isPersistent: false);

            if (signInResult.Succeeded)
                return RedirectToAction("Index", "Home");

            // First time: create a local IdentityUser linked to the external login
            var user = await CreateOrLinkUserFromExternalLogin(externalLoginInfo);
            if (user is not null)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Could not sign in with the external provider.");
            return RedirectToAction(nameof(Login));
        }

        private async Task<IdentityUser?> CreateOrLinkUserFromExternalLogin(
            ExternalLoginInfo info)
        {
            // Prefer the 'email' claim sent by Duende; fall back to standard ClaimTypes.Email
            var emailClaim =
                info.Principal.FindFirst("email") ??
                info.Principal.FindFirst(System.Security.Claims.ClaimTypes.Email);

            if (emailClaim is null) return null;

            var email = emailClaim.Value;
            var user = await _userManager.FindByEmailAsync(email);

            if (user is null)
            {
                user = new IdentityUser { UserName = email, Email = email };
                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded) return null;
            }

            var addLoginResult = await _userManager.AddLoginAsync(user, info);
            return addLoginResult.Succeeded ? user : null;
        }

        // ─── Registration ─────────────────────────────────────────────────────────

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new IdentityUser
                {
                    Email = model.Email,
                    UserName = model.UserName
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                    return RedirectToAction(nameof(Login));

                foreach (var error in result.Errors)
                    ModelState.AddModelError(error.Code, error.Description);
            }
            return View(model);
        }

        // ─── Logout ───────────────────────────────────────────────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            // Sign out of the local Identity cookie
            await _signInManager.SignOutAsync();

            // If the user logged in via Duende, also sign out of the OIDC session.
            // This sends an end-session request to https://localhost:5001/connect/endsession
            if (User.Identity?.AuthenticationType == "oidc")
            {
                return SignOut(
                    new AuthenticationProperties { RedirectUri = "/" },
                    "Cookies",
                    "oidc");
            }

            return RedirectToAction(nameof(Login));
        }
    }
}
