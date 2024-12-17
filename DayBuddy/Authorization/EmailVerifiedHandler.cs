using DayBuddy.Authorization.Requirements;
using DayBuddy.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DayBuddy.Authorization
{
    //acts as the answer to the EmailVerifiedRequirements question
    public class EmailVerifiedHandler : AuthorizationHandler<EmailVerifiedRequirement>
    {
        private readonly UserManager<DayBuddyUser> userManager;
        
        public EmailVerifiedHandler(UserManager<DayBuddyUser> userManager)
        {
            this.userManager = userManager;
        }

        protected async override Task HandleRequirementAsync(AuthorizationHandlerContext context, EmailVerifiedRequirement requirement)
        {
            //handle if the user is not authenticated
            if (context.User.Identity?.IsAuthenticated != true) return;
            
            DayBuddyUser? user = await userManager.GetUserAsync(context.User);

            if (user == null) return;

            if (user.EmailConfirmed)
            {
                context.Succeed(requirement);
                return;
            }

            if (context.Resource is HttpContext mvcContext)
            {
                // we still need to mark it as succeeded, 
                // else the responde code is 401 which means unauthorized
                context.Succeed(requirement);

                //redirect to another page cuz it failed the requirement
                mvcContext.Response.Redirect("/Account/VerifyEmail", false);
            }
        }
    }
}
