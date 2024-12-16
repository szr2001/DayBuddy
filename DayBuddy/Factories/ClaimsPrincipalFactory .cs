using DayBuddy.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace DayBuddy.Factories
{
    //factory method do add custom data in the user browser cookie to act as a cache to limit the calls to the db for some data
    //people might be able to edit the cookie, I didn't add special encription
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
