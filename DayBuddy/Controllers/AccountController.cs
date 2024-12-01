using DayBuddy.Models;
using DayBuddy.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Nodes;

namespace DayBuddy.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<DayBuddyUser> userManager;
        private readonly SignInManager<DayBuddyUser> signInManager;
        private readonly UserProfileValidatorService userProfileValidatorService;
        private readonly ProfanityFilterService profanityFilterService;
        public AccountController(UserManager<DayBuddyUser> userManager, SignInManager<DayBuddyUser> signInManager, UserProfileValidatorService userProfileValidatorService, ProfanityFilterService profanityFilterService)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.userProfileValidatorService = userProfileValidatorService;
            this.profanityFilterService = profanityFilterService;
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
        [HttpPost]
        public async Task<JsonResult> EditName([MaxLength(20, ErrorMessage = "Name too long")]
        [MinLength(5, ErrorMessage = "Name too short")] [Required]string newName)
        {
            if (!ModelState.IsValid)
            {
                var errorMessages = ModelState.Values
                                    .SelectMany(v => v.Errors)
                                    .Select(e => e.ErrorMessage)
                                    .ToArray();
                return Json(new { success = false, errors = errorMessages });
            }
            DayBuddyUser? user = await userManager.GetUserAsync(User);
            if(user == null)
            {
                return Json(new { success = false, errors = new[] { "User doesn't exist" } });
            }

            if (profanityFilterService.ContainsProfanity(newName))
            {
                return Json(new { success = false, errors = new[] { "Name cannot contain profanity" } });
            }

            user.UserName = newName;
            user.NormalizedUserName = userManager.NormalizeName(newName);

            await userManager.UpdateAsync(user);

            return Json(new { success = true, errors = Array.Empty<string>() });
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
                Premium = true
            };

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
