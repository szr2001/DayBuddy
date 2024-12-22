using MongoDbGenericRepository.Attributes;
using System.ComponentModel.DataAnnotations;

namespace DayBuddy.Models
{
    [CollectionName("Feedback")]
    public class Feedback
    {
        public Guid Id;
        public string? Content { get; set; }
        public Guid? SenderId { get; set; }

        public Feedback(string? content, Guid? senderId)
        {
            Id = Guid.NewGuid();
            Content = content;
            SenderId = senderId;
        }
    }
}
