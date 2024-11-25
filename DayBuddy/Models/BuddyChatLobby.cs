using MongoDbGenericRepository.Attributes;
using System.Configuration;

namespace DayBuddy.Models
{
    [CollectionName("ActiveBuddyLobbies")]
    public class BuddyChatLobby
    {
        public string Id { get; set; }
        public string[] Users { get; set; }
        public DateTime CreatedDate { get; set; }   
        public BuddyChatLobby(string[] users)
        {
            CreatedDate = DateTime.UtcNow;
            Id = Guid.NewGuid().ToString();
            Users = users;
        }
    }
}
