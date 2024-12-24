using MongoDbGenericRepository.Attributes;
using System.ComponentModel.DataAnnotations;

namespace DayBuddy.Models
{
    [CollectionName("Reports")]
    public class UserReport
    {
        public Guid Id { get; set; }
        public string? Reason { get; set; }
        public Guid UserId { get; set; }
        public Guid ReportedUserId { get; set; }
        public DateTime ReportDate { get; set; }

        public UserReport(string? reason, Guid userId, Guid reportedUserId)
        {
            ReportDate = DateTime.UtcNow;
            Id = Guid.NewGuid();
            Reason = reason;
            UserId = userId;
            ReportedUserId = reportedUserId;
        }
    }
}
