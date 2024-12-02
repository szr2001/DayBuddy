using AspNetCore.Identity.MongoDbCore;
using AspNetCore.Identity.MongoDbCore.Models;
using MongoDbGenericRepository.Attributes;
using System.ComponentModel.DataAnnotations;

namespace DayBuddy.Models
{
    [CollectionName("Users")]
    public class DayBuddyUser : MongoIdentityUser<Guid>
    {
        public int Age { get; set; } = 18;
        public int[] AgeRange { get; set; } = { 18, 100 }; 
        public string[] Interests { get; set; } = ["Programming", "Movies", "Sleeping", "Reading", "Dogs"];
        public string? Gender { get; set; } = "None";
        public float Score { get; set; } = 8;
        public string? Sexuality { get; set; } = "None";
        public string? Country { get; set; } = "None";
        public string? City { get; set; } = "None";
        public DateTime? LastTimeOnline { get; set; }
        public DateTime? PurchasedPremium { get; set; }
        public DateTime? MatchedWithBuddy { get; set; }
        public bool IsAvailable { get; set; }
        public Guid BuddyChatLobbyID{ get; set; }
    }
}
