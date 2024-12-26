using MongoDbGenericRepository.Attributes;

namespace DayBuddy.Models
{
    [CollectionName("DayBuddyStats")]
    public class DayBuddyStats
    {
        public Guid Id;
        public float TotalRevenue { get; set; }
        public float TotalExpenses { get; set; }
        public float ExpectedExpenses { get; set; }
        public DateTime LastTimeUpdated { get; set; }
    
        public DayBuddyStats() 
        {
            Id = Guid.NewGuid();
        }
    }
}
