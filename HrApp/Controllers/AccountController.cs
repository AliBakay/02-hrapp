using HrApp.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HrApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        #region Login
        public IActionResult Login()
        {
            return View();
        }
        #endregion

        #region Login Username

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
                var searchUser = await _userManager.FindByNameAsync(model.UserName);
                if (searchUser is not null)
                {
                    var result = await _signInManager.PasswordSignInAsync(searchUser, model.Password, false, lockoutOnFailure: false);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Invalid login attempt");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Invalid login attempt");
                }
            }
            return View(model);
        }

        #endregion

        #region Login Email

        [HttpGet]
        public IActionResult LoginEmail()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> LoginEmailAsyc(LoginEmailViewModel model)
        {
            if (ModelState.IsValid)
            {
                var searchUser = await _userManager.FindByEmailAsync(model.Email);
                if (searchUser is not null)
                {
                    var result = await _signInManager.PasswordSignInAsync(searchUser, model.Password, false, lockoutOnFailure: false);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Invalid login attempt");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Invalid login attempt");
                }
            }
            return View(model);
        }

        #endregion

        #region LoginExternalProvider
        public IActionResult LoginExternalProvider()
        {
            string? redirectUrl = Url.Action("ExternalProviderResponse", "Account");
            string scheme = "oidc";
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(
                    scheme, redirectUrl);
            return new ChallengeResult(scheme, properties);
        }
        #endregion
        #region ExternalProviderResponse
        public async Task<IActionResult> ExternalProviderResponse()
        {
            ExternalLoginInfo? externalLoginInfo =
                await _signInManager.GetExternalLoginInfoAsync();
            if (externalLoginInfo == null)
            {
                return RedirectToAction(nameof(Login));
            }
            else
            {
                var user = await _userManager.FindByLoginAsync(externalLoginInfo.LoginProvider, externalLoginInfo.ProviderKey);
                if (user == null)
                {
                    user = await CreateIdentityUserFromClaims(externalLoginInfo);
                }
                await _signInManager.SignInAsync(user, true);
            }
            return RedirectToAction("Index", "Home");
        }

        private async Task<IdentityUser?> CreateIdentityUserFromClaims(ExternalLoginInfo externalLoginInfo)
        {
            var claim = externalLoginInfo.Principal.FindFirst("email");
            if (claim != null)
            {
                var email = claim.Value;
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    user = new IdentityUser { UserName = email, Email = email };
                    var result = await _userManager.CreateAsync(user);
                    if (!result.Succeeded)
                    {
                        return null;
                    }
                }
                var loginResult = await _userManager.AddLoginAsync(user, externalLoginInfo);
                if (loginResult.Succeeded)
                {
                    return user;
                }
            }
            return null;
        }
        #endregion

        #region Register

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RegisterAsync(RegisterViewModel registerModel)
        {
            if (ModelState.IsValid)
            {
                var identityUser = new IdentityUser
                {
                    Email = registerModel.Email,
                    UserName = registerModel.UserName
                };
                var result = await _userManager.CreateAsync(identityUser, registerModel.Password);
                if (result.Succeeded)
                {
                    return RedirectToAction("Login", "Account");
                }
                else
                {
                    foreach(var error in result.Errors)
                    {
                        ModelState.AddModelError(error.Code, error.Description);
                    }
                }
            }
            return View();
        }

        #endregion

        #region Logout

        public async Task<IActionResult> LogoutAsync()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }

        #endregion
    }
}
