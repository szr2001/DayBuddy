using DayBuddy.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace DayBuddy.Filters
{
    /// <summary>
    /// Redirects the user to the LogOut to clear the cookie which will redirect it to the Login
    /// If the user exists it saves it in the context.HttpContext.Items with the key User
    /// So it can be retrieved in the controller by writting this
    /// DayBuddyUser user = (DayBuddyUser)HttpContext.Items[User]!;
    /// </summary>
    public class EnsureDayBuddyUserNotNullFilter : IAsyncActionFilter
    {
        private readonly UserManager<DayBuddyUser> _userManager;

        public EnsureDayBuddyUserNotNullFilter(UserManager<DayBuddyUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var user = await _userManager.GetUserAsync(context.HttpContext.User);

            if (user == null)
            {
                var controller = (Controller)context.Controller;
                context.Result = controller.RedirectToAction("LogOut", "Account");
                return;
            }

            context.HttpContext.Items[context.HttpContext.User] = user;
            await next();
        }
    }
}
