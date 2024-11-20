using DayBuddy.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DayBuddy.Controllers
{
    [Authorize]
    public class DayBuddyController : Controller
    {
        private readonly UserManager<DayBuddyUser> userManager;

        public DayBuddyController(UserManager<DayBuddyUser> userManager)
        {
            this.userManager = userManager;
        }

        public async Task<IActionResult> SearchBuddy()
        {
                //return RedirectToAction(nameof(BuddyChat));
            DayBuddyUser? user = await userManager.GetUserAsync(User);

            if (user?.Buddy != null)
            {
                return RedirectToAction(nameof(BuddyChat));
            }

            ViewBag.IsAvailable = user == null ? false : user.IsAvailable;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SearchBuddy(bool available)
        {
            DayBuddyUser? user = await userManager.GetUserAsync(User);
            if (user != null)
            {
                if(user.IsAvailable != available)
                {
                    user.IsAvailable = available;
                    await userManager.UpdateAsync(user);
                }
            }

            return RedirectToAction(nameof(SearchBuddy));
        }

        public IActionResult BuddyChat()
        {
            return View();
        }
    }
}
