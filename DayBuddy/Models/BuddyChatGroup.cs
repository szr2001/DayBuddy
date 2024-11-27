using MongoDbGenericRepository.Attributes;
using System.Configuration;

namespace DayBuddy.Models
{
    [CollectionName("ActiveBuddyGroups")]
    public class BuddyChatGroup
    {
        public Guid Id { get; set; }
        public Guid[] Users { get; set; }
        public DateTime CreatedDate { get; set; }   
        public BuddyChatGroup(Guid[] users)
        {
            CreatedDate = DateTime.UtcNow;
            Id = Guid.NewGuid();
            Users = users;
        }
    }
}
