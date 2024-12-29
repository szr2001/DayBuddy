using DayBuddy.Models;
using DayBuddy.Services;
using DayBuddy.Services.Caches;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Diagnostics;

namespace DayBuddy.Controllers
{
    [EnableRateLimiting("GeneralPolicy")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> logger;
        private readonly UserManager<DayBuddyUser> userManager;
        private readonly StatisticsCache statisticsCache;
        public HomeController(ILogger<HomeController> logger, UserManager<DayBuddyUser> userManager, StatisticsCache statisticsCache)
        {
            this.logger = logger;
            this.userManager = userManager;
            this.statisticsCache = statisticsCache;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> GetStarted()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                DayBuddyUser? user = await userManager.GetUserAsync(User);
                if(user != null)
                {
                    DateTime time = DateTime.UtcNow.AddDays(-1);
                    if(user.LastTimeOnline < time)
                    {
                        statisticsCache.ActiveUsers++;
                    }
                    user.LastTimeOnline = DateTime.UtcNow;
                    await userManager.UpdateAsync(user);
                    return RedirectToAction("Profile","Account");
                }
            }
            return View();
        }

        public IActionResult TermsAndConditions()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
