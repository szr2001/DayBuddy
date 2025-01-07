using DayBuddy.Models;
using DayBuddy.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DayBuddy.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly FeedbackService feedbackService;
        public AdminController(FeedbackService feedbackService)
        {
            this.feedbackService = feedbackService;
        }

        public IActionResult AdminPannel()
        {
            return View();
        }

        public async Task<IActionResult> ReadFeedback()
        {
            Feedback? feedback = await feedbackService.ExtractRandomMessageAsync();

            if(feedback != null)
            {
                ViewBag.Feedback = feedback;
            }

            return View();
        }
    }
}
