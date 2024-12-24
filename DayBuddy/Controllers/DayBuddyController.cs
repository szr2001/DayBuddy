using DayBuddy.Filters;
using DayBuddy.Models;
using DayBuddy.Services;
using DayBuddy.Services.Caches;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.DotNet.MSIdentity.Shared;
using Stripe.Reporting;
using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace DayBuddy.Controllers
{
    [Authorize("EmailVerified")]
    [EnableRateLimiting("GeneralPolicy")]
    [ServiceFilter(typeof(EnsureDayBuddyUserNotNullFilter))]
    public class DayBuddyController : Controller
    {
        //move some methods inside services
        private readonly UserManager<DayBuddyUser> userManager;
        private readonly ChatGroupsService chatGroupsService;
        private readonly MessagesService messagesService;
        private readonly UserReportService userReportService;
        private readonly UserService userService;
        private readonly FeedbackService feedbackService;
        private readonly BuddyGroupCacheService buddyGroupCacheService;

        private readonly int messageHistoryLength = 30;
        public DayBuddyController(UserManager<DayBuddyUser> userManager, ChatGroupsService chatLobbysService, UserService userService, BuddyGroupCacheService buddyGroupCacheService, MessagesService messagesService, FeedbackService feedbackService, UserReportService userReportService)
        {
            this.userManager = userManager;
            this.chatGroupsService = chatLobbysService;
            this.userService = userService;
            this.buddyGroupCacheService = buddyGroupCacheService;
            this.messagesService = messagesService;
            this.feedbackService = feedbackService;
            this.userReportService = userReportService;
        }

        [HttpPost]
        public async Task<JsonResult> ReportDayBuddy(string reason)
        {
            if (reason.Length > 20)
            {
                return Json(new {success = false, errors = new[]{"The reason must be less than 20 characters"} });
            }

            DayBuddyUser user = (DayBuddyUser)HttpContext.Items[User]!;

            if (user.BuddyChatGroupID == Guid.Empty) 
            {
                return Json(new { success = false, errors = new[] { "User is not in a chat group" } });
            }

            string[] groupUsers = buddyGroupCacheService.GetUsersInGroup(user.BuddyChatGroupID.ToString());

            string? buddyId = groupUsers.Where(id => id != user.Id.ToString()).FirstOrDefault();
            if (string.IsNullOrEmpty(buddyId))
            {
                return Json(new { success = false, errors = new[] { "buddy id does not exist" } });
            }

            DayBuddyUser? reportedUser = await userManager.FindByIdAsync(buddyId);
            if(reportedUser == null)
            {
                return Json(new { success = false, errors = new[] { "user does not exist" } });
            }

            UserReport userReport = new(reason, user.Id, reportedUser.Id);

            user.ReportedUsers.Add(reportedUser.Id);

            await userManager.UpdateAsync(reportedUser);

            await userReportService.InsertReport(userReport);
            return Json(new { success = true, errors = Array.Empty<string>()});
        }

        public async Task<IActionResult> LeaveFeedback()
        {
            DayBuddyUser user = (DayBuddyUser)HttpContext.Items[User]!;
            ViewBag.FeedbackRecieved = await feedbackService.GetUserFeedbackCount(user) > 0;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> LeaveFeedback([Required][MinLength(20,ErrorMessage ="Not enough details, try adding at least 20 characters")][MaxLength(300,ErrorMessage = "Try keeping it shorter, max 300 characters")]string content)
        {
            DayBuddyUser user = (DayBuddyUser)HttpContext.Items[User]!;
            if(await feedbackService.GetUserFeedbackCount(user) > 0)
            {
                return RedirectToAction("Profile","Account");
            }
            if (ModelState.IsValid)
            {
                Feedback feedback = new(content, user.Id);

                await feedbackService.InsertAsync(feedback);

                ViewBag.FeedbackRecieved = true;
            }
            return View();
        }

        [HttpGet]
        public async Task<JsonResult> GetBuddyMessages(int offset)
        {
            DayBuddyUser user = (DayBuddyUser)HttpContext.Items[User]!;

            if (user.BuddyChatGroupID == Guid.Empty)
            {
                return Json(new { success = false, errors = new[] { "User is not inside a Group" } });
            }

            List<GroupMessage> messages = await messagesService.GetGroupMessageInGroupAsync(user.BuddyChatGroupID, user, offset, messageHistoryLength);
            messages.Reverse();

            return Json(new { success = true, errors = Array.Empty<string>(), messagesFound = messages });
        }

        public IActionResult SearchBuddy()
        {
            DayBuddyUser user = (DayBuddyUser)HttpContext.Items[User]!;

            if (user.BuddyChatGroupID != Guid.Empty)
            {
                return RedirectToAction(nameof(BuddyChat));
            }

            if (userService.IsUserOnBuddySearchCooldown(user) && !userService.IsPremiumUser(user))
            {
                return RedirectToAction(nameof(BuddyCooldown));
            }
            
            ViewBag.IsAvailable = user.IsAvailable;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SearchBuddy(bool available)
        {
            DayBuddyUser user = (DayBuddyUser)HttpContext.Items[User]!;
            //get a random user which is available
            //if none found, then continue

            //if one found then move them to the Chat
            //get his info and update the chat view
            //Create a lobby with those two and assign them
            //save it in the database
            //set the users LobbyId to the new saved one
            if (user.BuddyChatGroupID != Guid.Empty)
            {
                return RedirectToAction(nameof(BuddyChat));
            }

            if (userService.IsUserOnBuddySearchCooldown(user) && !userService.IsPremiumUser(user))
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
                DayBuddyUser? buddyUser = await userService.GetBuddyMatchForProfileAsync(user);
                if (buddyUser != null && buddyUser.Id != user.Id)
                {
                    await chatGroupsService.AddBuddyGroup(user, buddyUser);
                    return RedirectToAction(nameof(BuddyChat));
                }
            }

            return RedirectToAction(nameof(SearchBuddy));
        }

        public IActionResult BuddyCooldown()
        {
            DayBuddyUser user = (DayBuddyUser)HttpContext.Items[User]!;

            if (!userService.IsUserOnBuddySearchCooldown(user) || userService.IsPremiumUser(user))
            {
                return RedirectToAction(nameof(SearchBuddy));
            }

            ViewBag.Cooldown = userService.GetUserBuddySearchCooldown(user);
            return View();
        }

        public async Task<IActionResult> UnmatchBuddy()
        {
            DayBuddyUser user = (DayBuddyUser)HttpContext.Items[User]!;

            if (user.BuddyChatGroupID != Guid.Empty)
            {
                string GroupId = user.BuddyChatGroupID.ToString();
                await chatGroupsService.RemoveBuddyGroup(user.BuddyChatGroupID);
            }

            return RedirectToAction(nameof(SearchBuddy));
        }

        public async Task<IActionResult> BuddyChat()
        {
            DayBuddyUser user = (DayBuddyUser)HttpContext.Items[User]!;

            if(user.BuddyChatGroupID == Guid.Empty)
            {
                return RedirectToAction(nameof(SearchBuddy));
            }

            string[] groupUsers = buddyGroupCacheService.GetUsersInGroup(user.BuddyChatGroupID.ToString());
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

            ViewBag.IsPremium = userService.IsPremiumUser(user);

            ViewBag.Cooldown = userService.GetUserBuddySearchCooldown(user);
            ViewBag.UserId = user.Id.ToString();

            UserProfile buddyProfile = userService.GetUserProfile(buddyUser);

            return View(buddyProfile);
        }
    }
}
