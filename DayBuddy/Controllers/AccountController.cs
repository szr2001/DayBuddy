using DayBuddy.Models;
using DayBuddy.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Nodes;

namespace DayBuddy.Controllers
{
    //maybe move edit logic in another controller?
    public class AccountController : Controller
    {
        private readonly UserManager<DayBuddyUser> userManager;
        private readonly SignInManager<DayBuddyUser> signInManager;
        private readonly UserService userService;
        public AccountController(UserManager<DayBuddyUser> userManager, SignInManager<DayBuddyUser> signInManager, UserService userService)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.userService = userService;
        }

        public IActionResult Login()
        {
            return View();
        }

        [Authorize]
        public async Task<IActionResult> Premium()
        {
            DayBuddyUser? user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction(nameof(Login));
            }

            ViewBag.IsPremium = userService.IsPremiumUser(user);

            return View();
        }

        [Authorize]
        public async Task<IActionResult> PurchasePremium()
        {
            DayBuddyUser? user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction(nameof(Login));
            }

            if (!userService.IsPremiumUser(user))
            {
                user.PurchasedPremium = DateTime.UtcNow;

                await userManager.UpdateAsync(user);
            }

            return RedirectToAction(nameof(Profile));
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login([Required][EmailAddress] string email, [Required] string password)
        {
            if (ModelState.IsValid)
            {
                DayBuddyUser? user = await userManager.FindByEmailAsync(email);
                if (user != null)
                {
                    Microsoft.AspNetCore.Identity.SignInResult result = await signInManager.PasswordSignInAsync(user, password, false, false);
                    user.LastTimeOnline = DateTime.UtcNow;
                    await userManager.UpdateAsync(user);

                    if (result.Succeeded)
                    {
                        return RedirectToAction("Profile", "Account");
                    }
                }
            }
            ModelState.AddModelError("", "Login Failed, invalid email or password");
            return View();
        }

        [Authorize]
        public async Task<IActionResult> Profile()
        {
            DayBuddyUser? user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction(nameof(Login));
            }

            UserProfile profileData = userService.GetUserProfile(user);

            return View(profileData);
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register(User user)
        {
            if (ModelState.IsValid)
            {
                //check for passwords be the same
                DayBuddyUser newUser = new()
                {
                    UserName = user.Name,
                    Email = user.Email,
                    LastTimeOnline = DateTime.UtcNow,
                };
                IdentityResult result = await userManager.CreateAsync(newUser, user.Password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(newUser, "User");
                    return RedirectToAction(nameof(Login));
                }
                else
                {
                    foreach (IdentityError error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }

            return View(user);
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        [Authorize]
        public async Task<IActionResult> LogOut()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}
