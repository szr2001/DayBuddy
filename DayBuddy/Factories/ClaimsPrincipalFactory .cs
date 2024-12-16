using DayBuddy.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace DayBuddy.Factories
{
    //add custom data to the Identity claim to avoid making multiple calls to the database
    //can also be used with JWT tokens auth tokens
    public class DayBuddyUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<DayBuddyUser>
    {
        public DayBuddyUserClaimsPrincipalFactory(UserManager<DayBuddyUser> userManager, IOptions<IdentityOptions> optionsAccessor)
            : base(userManager, optionsAccessor) { }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(DayBuddyUser user)
        {
            var identity = await base.GenerateClaimsAsync(user);

            //we save if the Emaill is confirmed, so we can check it in the future without making a call to the db
            identity.AddClaim(new Claim("EmailConfirmed", user.EmailConfirmed.ToString()));

            return identity;
        }
    }
}
