using DayBuddy.Hubs;
using DayBuddy.Models;
using DayBuddy.Services;
using DayBuddy.Services.Caches;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Connections;
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
        private readonly MessagesService messagesService;
        private readonly UserService userService;
        private readonly BuddyGroupCacheService buddyGroupCacheService;

        private readonly int messageHistoryLength = 50;
        public DayBuddyController(UserManager<DayBuddyUser> userManager, ChatGroupsService chatLobbysService, UserService userService, BuddyGroupCacheService buddyGroupCacheService, MessagesService messagesService)
        {
            this.userManager = userManager;
            this.chatLobbysService = chatLobbysService;
            this.userService = userService;
            this.buddyGroupCacheService = buddyGroupCacheService;
            this.messagesService = messagesService;
        }

        [HttpPost]
        public async Task<JsonResult> GetBuddyMessages(int offset)
        {
            DayBuddyUser? authUser = await userManager.GetUserAsync(User);
            if(authUser == null)
            {
                return Json(new { success = false, errors = new[] { "User doesn't exist" } });
            }
            if(authUser.BuddyChatGroupID == Guid.Empty)
            {
                return Json(new { success = false, errors = new[] { "User is not inside a Group" } });
            }

            return Json(messagesService.GetGroupMessageInGroupAsync(authUser.BuddyChatGroupID, authUser, offset, messageHistoryLength));
        }

        public async Task<IActionResult> SearchBuddy()
        {
            DayBuddyUser? user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }
            if (user.BuddyChatGroupID != Guid.Empty)
            {
                return RedirectToAction(nameof(BuddyChat));
            }

            if (userService.IsUserOnBuddySearchCooldown(user))
            {
                return RedirectToAction(nameof(BuddyCooldown));
            }
            
            ViewBag.IsAvailable = user.IsAvailable;

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
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }
            if (user.BuddyChatGroupID != Guid.Empty)
            {
                return RedirectToAction(nameof(BuddyChat));
            }

            if (userService.IsUserOnBuddySearchCooldown(user))
            {
                return RedirectToAction(nameof(BuddyCooldown));
            }

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

        public async Task<IActionResult> BuddyCooldown()
        {
            DayBuddyUser? user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (!userService.IsUserOnBuddySearchCooldown(user))
            {
                return RedirectToAction(nameof(SearchBuddy));
            }
            ViewBag.Cooldown = userService.GetUserBuddySearchCooldown(user);
            return View();
        }

        public async Task<IActionResult> UnmatchBuddy()
        {
            DayBuddyUser? user = await userManager.GetUserAsync(User);

            if(user == null)
            {
                return RedirectToAction("Login","Account");
            }
            if(user.BuddyChatGroupID != Guid.Empty)
            {
                string GroupId = user.BuddyChatGroupID.ToString();
                await chatLobbysService.RemoveBuddyGroup(user.BuddyChatGroupID);
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

            string? groupId = buddyGroupCacheService.GetUserGroup(user.Id.ToString());

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

            ViewBag.Cooldown = userService.GetUserBuddySearchCooldown(user);

            UserProfile buddyProfile = userService.GetUserProfile(buddyUser);
            List<GroupMessage> messages = await messagesService.GetGroupMessageInGroupAsync(user.BuddyChatGroupID, user, 0, messageHistoryLength);

            GroupChat groupChat = new(buddyProfile, messages);
            return View(groupChat);
        }
    }
}
