using System.Runtime.InteropServices;

namespace DayBuddy.Services.Caches
{
    public class StatisticsCache
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int PremiumUsers { get; set; }
        public int TotalReports { get; set; }
        public int TotalBans { get; set; }
        public float TotalRevenue { get; set; }
        public float TotalExpenses { get; set; }
        public float ExpectedExpenses { get; set; }
        public float Profits 
        {
            get 
            {
                return TotalRevenue - TotalExpenses;
            }
        }
        public string DayBudduyFuture { get; set; } = "Uncertain";
    }
}
