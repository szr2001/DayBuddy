using DayBuddy.Authorization.Requirements;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DayBuddy.Authorization
{
    //acts as the answer to the EmailVerifiedRequirements question
    public class EmailVerifiedHandler : AuthorizationHandler<EmailVerifiedRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, EmailVerifiedRequirement requirement)
        {
            //handle if the user is not authenticated
            if (context.User.Identity?.IsAuthenticated != true)
            {
                return Task.CompletedTask;
            }

            //get the cookie data, less secure but also less calls to the db to remain in the free tier
            var emailConfirmedClaim = context.User.FindFirst("EmailConfirmed");

            if (emailConfirmedClaim != null && bool.TryParse(emailConfirmedClaim.Value, out var isEmailConfirmed) && isEmailConfirmed)
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            if (context.Resource is HttpContext mvcContext)
            {
                // we still need to mark it as succeeded, 
                // else the responde code is 401 which means unauthorized
                context.Succeed(requirement);

                //redirect to another page cuz it failed the requirement
                mvcContext.Response.Redirect("/Home/Index", false);
                return Task.CompletedTask;
            }

            return Task.CompletedTask;

            //secure aproach but costly on the db calls

            //if (context.User.Identity?.IsAuthenticated != true) return;

            //DayBuddyUser? user = await userManager.GetUserAsync(context.User);

            //if (user == null) return;

            //if (user.EmailConfirmed)
            //{
            //    context.Succeed(requirement);
            //}
        }

    }
}
