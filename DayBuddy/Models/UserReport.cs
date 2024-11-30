using MongoDbGenericRepository.Attributes;
using System.ComponentModel.DataAnnotations;

namespace DayBuddy.Models
{
    [CollectionName("Reports")]
    public class UserReport
    {
        public Guid Id { get; set; }
        [MaxLength(50)]
        public string Reason { get; set; }
        public Guid UserId { get; set; }
        public Guid ReportedUserId { get; set; }

        public UserReport(string reason, Guid userId, Guid reportedUserId)
        {
            Id = Guid.NewGuid();
            Reason = reason;
            UserId = userId;
            ReportedUserId = reportedUserId;
        }
    }
}
