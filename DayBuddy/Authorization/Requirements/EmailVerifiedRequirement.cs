using Microsoft.AspNetCore.Authorization;

namespace DayBuddy.Authorization.Requirements
{
    //acts as the question for the EmailVerifiedHandler answer, as an Id
    public class EmailVerifiedRequirement : IAuthorizationRequirement
    {
    }
}
