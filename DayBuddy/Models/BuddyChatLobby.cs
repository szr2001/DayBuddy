using MongoDbGenericRepository.Attributes;
using System.Configuration;

namespace DayBuddy.Models
{
    [CollectionName("ActiveChats")]
    public class BuddyChatLobby
    {
        public string Id { get; set; }
        public string[] Users { get; set; }
        public BuddyChatLobby(string[] users)
        {
            Id = Guid.NewGuid().ToString();
            Users = users;
        }
    }
}
