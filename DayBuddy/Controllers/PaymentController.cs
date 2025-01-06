using DayBuddy.Filters;
using DayBuddy.Models;
using DayBuddy.Services;
using DayBuddy.Services.Caches;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using Stripe.Checkout;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DayBuddy.Controllers
{
    [Authorize("EmailVerified")]
    [EnableRateLimiting("GeneralPolicy")]
    [ServiceFilter(typeof(EnsureDayBuddyUserNotNullFilter))]
    public class PaymentController : Controller
    {
        private readonly UserManager<DayBuddyUser> userManager;
        private readonly UserService userService;
        private readonly StatisticsCache statisticsCache;
        public PaymentController(UserManager<DayBuddyUser> userManager, UserService userService, StatisticsCache statisticsCache)
        {
            this.userManager = userManager;
            this.userService = userService;
            this.statisticsCache = statisticsCache;
        }

        public IActionResult Premium()
        {
            DayBuddyUser user = (DayBuddyUser)HttpContext.Items[User]!;

            ViewBag.IsPremium = userService.IsPremiumUser(user);

            return View();
        }

        public async Task<IActionResult> PremiumCheckout()
        {
            DayBuddyUser user = (DayBuddyUser)HttpContext.Items[User]!;

            if (TempData.TryGetValue("PremiumPurchaseSession", out object? SessionId) && SessionId != null)
            {
                SessionService service = new();
                Session session = await service.GetAsync((string)SessionId);

                if(session.PaymentStatus == "paid")
                {
                    if (!userService.IsPremiumUser(user))
                    {
                        user.PremiumExpiryDate = DateTime.UtcNow.AddDays(30);
                        statisticsCache.PremiumUsers++;
                        statisticsCache.TotalRevenue += 8;
                        await userManager.UpdateAsync(user);
                    }
                    return RedirectToAction("Profile","Account");
                }
            }

            return RedirectToAction("Payment","Premium");
        }

        public async Task<IActionResult> PurchasePremium()
        {
            DayBuddyUser user = (DayBuddyUser)HttpContext.Items[User]!;

            if (userService.IsPremiumUser(user))
            {
                return RedirectToAction("Account", "Profile");
            }

            string hostAdress = $"{Request.Scheme}://{Request.Host}";

            SessionCreateOptions options = new()
            {
            SuccessUrl = $@"{hostAdress}/Payment/PremiumCheckout",
            CancelUrl = $@"{hostAdress}/Account/Profile",
            LineItems = new List<SessionLineItemOptions>() 
            {
                new SessionLineItemOptions()
                {
                    PriceData = new SessionLineItemPriceDataOptions()
                    {
                        UnitAmount = 800,
                        Currency = "eur",
                        ProductData = new SessionLineItemPriceDataProductDataOptions()
                        {
                            Name = "DayBuddy 30 Days Premium",
                        },
                    },
                    Quantity = 1,
                }
            },
            Mode = "payment",
            CustomerEmail = user.Email,
            };

            SessionService service = new();
            Session session = await service.CreateAsync(options);

            TempData["PremiumPurchaseSession"] = session.Id; 

            return Redirect(session.Url);
        }
    }
}
