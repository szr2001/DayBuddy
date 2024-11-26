using MongoDbGenericRepository.Attributes;
using System.Configuration;

namespace DayBuddy.Models
{
    [CollectionName("ActiveBuddyGroups")]
    public class BuddyChatGroup
    {
        public string Id { get; set; }
        public string[] Users { get; set; }
        public DateTime CreatedDate { get; set; }   
        public BuddyChatGroup(string[] users)
        {
            CreatedDate = DateTime.UtcNow;
            Id = Guid.NewGuid().ToString();
            Users = users;
        }
    }
}
