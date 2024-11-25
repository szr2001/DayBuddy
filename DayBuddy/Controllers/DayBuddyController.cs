using DayBuddy.Models;
using DayBuddy.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DayBuddy.Controllers
{
    [Authorize]
    public class DayBuddyController : Controller
    {
        private readonly UserManager<DayBuddyUser> userManager;
        private readonly ChatLobbysService chatLobbysService;
        private readonly UserCacheService userCacheService;
        public DayBuddyController(UserManager<DayBuddyUser> userManager, ChatLobbysService chatLobbysService, UserCacheService userCacheService)
        {
            this.userManager = userManager;
            this.chatLobbysService = chatLobbysService;
            this.userCacheService = userCacheService;
        }

        public async Task<IActionResult> SearchBuddy()
        {
                return RedirectToAction(nameof(BuddyChat));
            DayBuddyUser? user = await userManager.GetUserAsync(User);
            if (user?.BuddyChatLobbyID != null)
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
