using DayBuddy.Models;
using DayBuddy.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DayBuddy.Controllers
{
    [Authorize]
    public class PaymentController : Controller
    {
        private readonly UserManager<DayBuddyUser> userManager;
        private readonly UserService userService;

        public PaymentController(UserManager<DayBuddyUser> userManager, UserService userService)
        {
            this.userManager = userManager;
            this.userService = userService;
        }

        public async Task<IActionResult> Premium()
        {
            DayBuddyUser? user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.IsPremium = userService.IsPremiumUser(user);

            return View();
        }

        public async Task<IActionResult> PurchasePremium()
        {
            DayBuddyUser? user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (!userService.IsPremiumUser(user))
            {
                user.PurchasedPremium = DateTime.UtcNow;

                await userManager.UpdateAsync(user);
            }

            return RedirectToAction("Profile", "Account");
        }
    }
}
