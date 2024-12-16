﻿using DayBuddy.Authorization.Requirements;
using Microsoft.AspNetCore.Authorization;

namespace DayBuddy.Authorization
{
    //acts as the answer to the EmailVerifiedRequirements question
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
