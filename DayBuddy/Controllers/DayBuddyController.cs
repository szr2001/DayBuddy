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
        //move some methods inside services
        private readonly UserManager<DayBuddyUser> userManager;
        private readonly ChatGroupsService chatLobbysService;
        private readonly UserService userService;
        private readonly BuddyGroupCacheService buddyGroupCacheService;
        public DayBuddyController(UserManager<DayBuddyUser> userManager, ChatGroupsService chatLobbysService, UserService userService, BuddyGroupCacheService buddyGroupCacheService)
        {
            this.userManager = userManager;
            this.chatLobbysService = chatLobbysService;
            this.userService = userService;
            this.buddyGroupCacheService = buddyGroupCacheService;
        }

        public async Task<IActionResult> SearchBuddy()
        {
            DayBuddyUser? user = await userManager.GetUserAsync(User);
            if (user?.BuddyChatLobbyID != Guid.Empty)
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
            if (user == null || user.BuddyChatLobbyID != Guid.Empty) return RedirectToAction(nameof(SearchBuddy));

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
                    await chatLobbysService.AddBuddyGroup(user, buddyUser);
                    return RedirectToAction(nameof(BuddyChat));
                }
            }

            return RedirectToAction(nameof(SearchBuddy));
        }

        public async Task<IActionResult> UnmatchBuddy()
        {
            DayBuddyUser? user = await userManager.GetUserAsync(User);

            if(user == null)
            {
                return RedirectToAction("Login","Account");
            }
            if(user.BuddyChatLobbyID != Guid.Empty)
            {
                string GroupId = user.BuddyChatLobbyID.ToString();
                await chatLobbysService.RemoveBuddyGroup(user.BuddyChatLobbyID);
            }

            return RedirectToAction(nameof(SearchBuddy));
        }

        public async Task<IActionResult> BuddyChat()
        {
            DayBuddyUser? user = await userManager.GetUserAsync(User);
            if(user == null)
            {
               return RedirectToAction("Login","Account");
            }

            string groupId = buddyGroupCacheService.GetUserGroup(user.Id.ToString());

            if(string.IsNullOrEmpty(groupId))
            {
                //there has been an error so delete the group
                Console.WriteLine($"Group ID not found with user {user.UserName}");
                return RedirectToAction(nameof(SearchBuddy));
            }

            string[] groupUsers = buddyGroupCacheService.GetUsersInGroup(groupId);
            string? buddyId = groupUsers.Where(id => id != user.Id.ToString()).FirstOrDefault();
            if (string.IsNullOrEmpty(buddyId))
            {
                //there has been an error so delete the group
                Console.WriteLine($"Something is wrong when retriving the user from the group {user.UserName}");
                return RedirectToAction(nameof(SearchBuddy));
            }

            DayBuddyUser? buddyUser = await userManager.FindByIdAsync(buddyId);
            if(buddyUser == null)
            {
                Console.WriteLine($"Couldn't find user in the group with {user.UserName}");
                return RedirectToAction(nameof(SearchBuddy));
            }

            UserProfile buddyProfile = userService.GetUserProfile(buddyUser);
            return View(buddyProfile);
        }
    }
}
