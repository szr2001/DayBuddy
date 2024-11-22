using MongoDbGenericRepository.Attributes;

namespace DayBuddy.Models
{
    [CollectionName("Messages")]
    public class BuddyMessage
    {
        public string Message { get; set; }
        public string UserId { get; set; }
        public string ChatLobbyId { get; set; }
        
        public BuddyMessage(string message, string userId, string chatLobbyId)
        {
            Message = message;
            UserId = userId;
            ChatLobbyId = chatLobbyId;
        }
    }
}
