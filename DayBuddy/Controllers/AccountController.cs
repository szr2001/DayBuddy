using DayBuddy.Models;
using DayBuddy.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using System.ComponentModel.DataAnnotations;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DayBuddy.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<DayBuddyUser> userManager;
        private readonly SignInManager<DayBuddyUser> signInManager;
        private readonly UserProfileValidatorService userProfileValidatorService;
        public AccountController(UserManager<DayBuddyUser> userManager, SignInManager<DayBuddyUser> signInManager, UserProfileValidatorService userProfileValidatorService)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.userProfileValidatorService = userProfileValidatorService;
        }

        public IActionResult Login()
        {
            return View();
        }

        [Authorize]
        public IActionResult Premium()
        {
            return View();
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

            UserProfile profileData = new()
            {
                Name = user.UserName,
                Sexuality = user.Sexuality,
                Age = user.Age,
                Interests = user.Interests,
                Gender = user.Gender,
                Premium = false
            };

            return View(profileData);
        }

        [Authorize]
        public async Task<IActionResult> EditProfile()
        {
            DayBuddyUser? user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction(nameof(Login));
            }

            UserProfile profileData = new()
            {
                Name = user.UserName,
                Sexuality = user.Sexuality,
                Age = user.Age,
                Interests = user.Interests,
                Gender = user.Gender,
                Premium = false
            };

            ViewBag.Genders = userProfileValidatorService.Genders.ToList();
            ViewBag.Sexualities = userProfileValidatorService.Sexualities.ToList();
            ViewBag.Interests = userProfileValidatorService.Interests.ToList();

            return View(profileData);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> EditProfile(UserProfile profile)
        {
            if (ModelState.IsValid)
            {
                profile = userProfileValidatorService.ValidateUserProfile(profile);
                DayBuddyUser? user = await userManager.GetUserAsync(User);
                if (user == null)
                {
                    return RedirectToAction(nameof(Login));
                }

                user.Sexuality = profile.Sexuality;
                user.Age = profile.Age;
                user.Interests = profile.Interests;
                user.Gender = profile.Gender;

                IdentityResult result = await userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    foreach (IdentityError error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
                ViewBag.Genders = userProfileValidatorService.Genders.ToList();
                ViewBag.Sexualities = userProfileValidatorService.Sexualities.ToList();
                ViewBag.Interests = userProfileValidatorService.Interests.ToList();
            }
            return View(profile);
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
