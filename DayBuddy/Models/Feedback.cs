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

        public Feedback() 
        {
            Id = Guid.NewGuid();
        }
    }
}
