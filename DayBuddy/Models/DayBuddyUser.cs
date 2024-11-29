using AspNetCore.Identity.MongoDbCore;
using AspNetCore.Identity.MongoDbCore.Models;
using MongoDbGenericRepository.Attributes;

namespace DayBuddy.Models
{
    [CollectionName("Users")]
    public class DayBuddyUser : MongoIdentityUser<Guid>
    {
        public int Age { get; set; }
        public int[] AgeRange { get; set; } = { 18, 100 }; 
        public string[] Interests { get; set; } = [];
        public string? Gender { get; set; }
        public string? Sexuality { get; set; }
        public DateTime? LastTimeOnline { get; set; }
        public DateTime? PurchasedPremium { get; set; }
        public DateTime? MatchedWithBuddy { get; set; }
        public bool IsAvailable { get; set; }
        public Guid BuddyChatLobbyID{ get; set; }
    }
}
