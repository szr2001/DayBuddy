using MongoDbGenericRepository.Attributes;

namespace DayBuddy.Models
{
    [CollectionName("Messages")]
    public class BuddyMessage
    {
        public Guid Id { get; set; }
        public string Message { get; set; }
        public Guid SenderId { get; set; }
        public Guid ChatGroupId { get; set; }
        public DateTime CreatedDate { get; set; }

        public BuddyMessage(string message, Guid userId, Guid chatGroupId)
        {
            CreatedDate = DateTime.UtcNow;
            Id = Guid.NewGuid();
            Message = message;
            SenderId = userId;
            ChatGroupId = chatGroupId;
        }
    }
}
