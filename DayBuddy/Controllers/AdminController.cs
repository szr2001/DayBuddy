using DayBuddy.Models;
using DayBuddy.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DayBuddy.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly FeedbackService feedbackService;
        private readonly GmailSMTPEmailService gmailSMTPEmailService;
        private readonly UserManager<DayBuddyUser> userManager;
        private readonly UserService userService;
        public AdminController(FeedbackService feedbackService, GmailSMTPEmailService gmailSMTPEmailService, UserManager<DayBuddyUser> userManager, UserService userService)
        {
            this.feedbackService = feedbackService;
            this.gmailSMTPEmailService = gmailSMTPEmailService;
            this.userManager = userManager;
            this.userService = userService;
        }

        public IActionResult AdminPannel()
        {
            return View();
        }

        [HttpPost]
        public async Task<JsonResult> GiftPremium(int days)
        {
            Guid? senderId = (Guid?)TempData["FeedbackUserId"];

            if(senderId == null)
            {
                return Json(new { success = false, errors = new[] { "UserId is null" } });
            }
            
            if (days > 5)
            {
                TempData["FeedbackUserId"] = senderId;
                return Json(new { success = false, errors = new[] { "Can't give more than 5 days premium as a reward" } });
            }

            DayBuddyUser? user = await userManager.FindByIdAsync(senderId.ToString()!);

            if(user == null)
            {
                return Json(new { success = false, errors = new[] { "User with that Id doesn't exist" } });
            }

            //if the user already has premium, then add the days to the expiery date
            if (userService.IsPremiumUser(user))
            {
                user.PremiumExpiryDate = user.PremiumExpiryDate.AddDays(days);
            }
            else
            {
                //else set the Expiry date to Now, and add the amount of days.
                //because we don't know when was the last time this user had premium, it might be 1 month ago
                //in that case, if we just add +1 or +5, the user will still not have premium active.
                user.PremiumExpiryDate = DateTime.UtcNow.AddDays(days);
            }

            await userManager.UpdateAsync(user);

            return Json(new { success = true, errors = Array.Empty<string>() });
        }

        [HttpPost]
        public async Task<JsonResult> SendEmail(string content)
        {
            Guid? senderId = (Guid?)TempData["FeedbackUserId"];

            if (senderId == null)
            {
                return Json(new { success = false, errors = new[] { "UserId is null" } });
            }

            if (string.IsNullOrEmpty(content))
            {
                TempData["FeedbackUserId"] = senderId;
                return Json(new { success = false, errors = new[] { "Email content can't be null" } });
            }

            DayBuddyUser? user = await userManager.FindByIdAsync(senderId.ToString()!);

            if (user == null)
            {
                return Json(new { success = false, errors = new[] { "User with that Id doesn't exist" } });
            }

            bool emailSent = await gmailSMTPEmailService.TrySendEmailAsync
                (
                    user.Email!,
                    "DayBuddy Feedback Response", 
                    $"<html><body>{content}</body></html>"
                );

            if(emailSent)
            {
                return Json(new { success = true, errors = Array.Empty<string>() });
            }
            else
            {
                return Json(new { success = false, errors = new[] {"Can't send more emails right now, try again later" } });
            }
        }

        public async Task<IActionResult> ReadFeedback()
        {
            Feedback? feedback = await feedbackService.ExtractRandomMessageAsync();

            if(feedback != null)
            {
                ViewBag.Feedback = feedback.Content;
                TempData["FeedbackUserId"] = feedback.SenderId;
            }

            return View();
        }
    }
}
