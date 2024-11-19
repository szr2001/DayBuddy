using AspNetCore.Identity.MongoDbCore;
using AspNetCore.Identity.MongoDbCore.Models;
using MongoDbGenericRepository.Attributes;

namespace DayBuddy.Models
{
    [CollectionName("Users")]
    public class DayBuddyUser : MongoIdentityUser<Guid>
    {
        public DateTime? LastTimeOnline { get; set; }
        public DateTime? PurchasedPremium { get; set; }
        public DateTime? MatchedWithBuddy { get; set; }
        public bool IsAvailable { get; set; }
        public Guid? Buddy { get; set; }
    }
}
