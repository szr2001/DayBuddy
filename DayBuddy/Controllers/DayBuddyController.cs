using DayBuddy.Hubs;
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
        private readonly ChatGroupsService chatLobbysService;
        private readonly UserService userService;
        public DayBuddyController(UserManager<DayBuddyUser> userManager, ChatGroupsService chatLobbysService, UserService userService)
        {
            this.userManager = userManager;
            this.chatLobbysService = chatLobbysService;
            this.userService = userService;
        }

        public async Task<IActionResult> SearchBuddy()
        {
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
            //get a random user which is available
            //if none found, then continue

            //if one found then move them to the Chat
            //get his info and update the chat view
            //Create a lobby with those two and assign them
            //save it in the database
            //set the users LobbyId to the new saved one
            if (user == null || user.BuddyChatLobbyID != null) return RedirectToAction(nameof(SearchBuddy));

            if (user.IsAvailable != available)
            {
                user.IsAvailable = available;
                await userManager.UpdateAsync(user);
            }
            if (available)
            {
                DayBuddyUser? buddyUser = await userService.GetRndAvailableUserAsync(user);
                if (buddyUser != null && buddyUser.Id != user.Id)
                {
                    await chatLobbysService.ConnectUsers(user, buddyUser);
                    return RedirectToAction(nameof(BuddyChat));
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
