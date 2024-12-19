using DayBuddy.Filters;
using DayBuddy.Models;
using DayBuddy.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.ComponentModel.DataAnnotations;

namespace DayBuddy.Controllers
{
    [EnableRateLimiting("GeneralPolicy")]
    public class AccountController : Controller
    {
        private readonly UserManager<DayBuddyUser> userManager;
        private readonly SignInManager<DayBuddyUser> signInManager;
        private readonly UserService userService;
        private readonly GmailSMTPEmailService gmailService;
        public AccountController(UserManager<DayBuddyUser> userManager, SignInManager<DayBuddyUser> signInManager, UserService userService, GmailSMTPEmailService gmailService)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.userService = userService;
            this.gmailService = gmailService;
        }

        public IActionResult Login()
        {
            return View();
        }

        [Authorize]
        [ServiceFilter(typeof(EnsureDayBuddyUserNotNullFilter))]
        public IActionResult VerifyEmail()
        {
            DayBuddyUser user = (DayBuddyUser)HttpContext.Items[User]!;
            if (user.EmailConfirmed)
            {
                return RedirectToAction(nameof(Profile));
            }
            return View();
        }

        public async Task <IActionResult> ConfirmVerifyEmail(string userId, string token)
        {
            if (userId == null || token == null)
            {
                return BadRequest("Invalid email confirmation request.");
            }

            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"Unable to load user with specified Id");
            }
            if (user.EmailConfirmed)
            {
                return BadRequest("User Already Verified.");
            }

            var result = await userManager.ConfirmEmailAsync(user, token);
            
            if (result.Succeeded)
            {
                user.EmailConfirmed = true;
                await userManager.UpdateAsync(user);
                return RedirectToAction(nameof(Profile));
            }
            else
            {
                return BadRequest("Email verification failed.");
            }
        }

        [Authorize]
        [EnableRateLimiting("ReSendEmailPolicy")]
        [ServiceFilter(typeof(EnsureDayBuddyUserNotNullFilter))]
        public async Task<IActionResult> ReSendVerificationEmail()
        {
            DayBuddyUser user = (DayBuddyUser)HttpContext.Items[User]!;
            if (user.EmailConfirmed)
            {
                return RedirectToAction(nameof(Profile));
            }

            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);

            var confirmationLink = Url.Action("ConfirmVerifyEmail", "Account", new { userId = user.Id.ToString(), token }, Request.Scheme);

            bool emailSent = await gmailService.TrySendEmailAsync
                (
                    user.Email!, 
                    $"<html><body>Verify your DayBuddy account by clicking this link: <a href = '{confirmationLink}'>{confirmationLink}</a> </html></body>"
                );
            if (emailSent)
            {
                return View("VerifyEmail");
            }

            return BadRequest("Can't send an email today, try again later.");
        }

        [Authorize]
        [ServiceFilter(typeof(EnsureDayBuddyUserNotNullFilter))]
        public IActionResult ResetPassword()
        {
            return View();
        }

        [HttpPost]
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
        [ServiceFilter(typeof(EnsureDayBuddyUserNotNullFilter))]
        public IActionResult Profile()
        {
            DayBuddyUser user = (DayBuddyUser)HttpContext.Items[User]!;

            UserProfile profileData = userService.GetUserProfile(user);

            if (profileData.Premium)
            {
                ViewBag.PremiumDuration = userService.GetUserPremiumDurationLeft(user);
            }

            return View(profileData);
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
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

                    var token = await userManager.GenerateEmailConfirmationTokenAsync(newUser);

                    var confirmationLink = Url.Action("ConfirmVerifyEmail", "Account", new { userId = newUser.Id.ToString(), token }, Request.Scheme);

                    bool emailSent = await gmailService.TrySendEmailAsync
                        (
                            user.Email!,
                            $"<html><body>Welcome to DayBuddy, Verify your account by clicking this link: <a href = '{confirmationLink}'>{confirmationLink}</a> </html></body>"
                        );

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

        public async Task<IActionResult> LogOut()
        {
            //test
            DayBuddyUser? user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction(nameof(Login));
            }
            user.PurchasedPremium = null;

            await userManager.UpdateAsync(user);
            //test

            await signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}
