using DayBuddy.Authorization.Requirements;
using Microsoft.AspNetCore.Authorization;

namespace DayBuddy.Authorization
{
    //acts as the answer to the EmailVerifiedRequirements question
    //it can be used in [Authorize] attribute like [Authorize("EmailVerified")]
    //to only allow access to the method/controller to accounts that have emaill verified
    public class EmailVerifiedHandler : AuthorizationHandler<EmailVerifiedRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, EmailVerifiedRequirement requirement)
        {
            if (context.User.Identity?.IsAuthenticated != true)
            {
                return Task.CompletedTask;
            }

            var emailConfirmedClaim = context.User.FindFirst("EmailConfirmed");

            if (emailConfirmedClaim != null && bool.TryParse(emailConfirmedClaim.Value, out var isEmailConfirmed) && isEmailConfirmed)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
