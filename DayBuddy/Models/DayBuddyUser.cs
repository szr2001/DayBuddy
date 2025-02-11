﻿using AspNetCore.Identity.MongoDbCore.Models;
using MongoDbGenericRepository.Attributes;

namespace DayBuddy.Models
{
    [CollectionName("Users")]
    public class DayBuddyUser : MongoIdentityUser<Guid>
    {
        public int Age { get; set; } = 18;
        public int[] AgeRange { get; set; } = { 18, 100 }; 
        public string[] Interests { get; set; } = [];
        public string Gender { get; set; } = "";
        public float Score { get; set; } = 8;
        public string Sexuality { get; set; } = "";
        public string Country { get; set; } = "";
        public string City { get; set; } = "";
        public List<Guid> ReportedUsers { get; set; } = [];
        public DateTime LastTimeOnline { get; set; } = DateTime.UtcNow;
        public DateTime PremiumExpiryDate { get; set; } = DateTime.MinValue;
        public DateTime MatchedWithBuddy { get; set; } = DateTime.MinValue;
        public bool IsAvailable { get; set; }
        public Guid BuddyChatGroupID{ get; set; } = Guid.Empty;
    }
}
