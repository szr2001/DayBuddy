using DayBuddy.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace DayBuddy.Factories
{
    //add custom data to the Identity claim to avoid making multiple calls to the database
    //can also be used with JWT tokens auth tokens
    //factory method do add custom data in the user browser cookie to act as a cache to limit the calls to the db for some data
    //people might be able to edit the cookie, I didn't add special encription
    public class DayBuddyUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<DayBuddyUser>
    {
        public DayBuddyUserClaimsPrincipalFactory(UserManager<DayBuddyUser> userManager, IOptions<IdentityOptions> optionsAccessor)
            : base(userManager, optionsAccessor) { }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(DayBuddyUser user)
        {
            var identity = await base.GenerateClaimsAsync(user);
            identity.AddClaim(new Claim("ID", user.Id.ToString()));

            return identity;
        }
    }
}