namespace DayBuddy.Services.Caches
{
    public class StatisticsCache
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int PremiumUsers { get; set; }
        public int TotalReports { get; set; }
        public int TotalBans { get; set; }
        public int TotalRevenue { get; set; }
        public int TotalExpenses { get; set; }
        public int ExpectedExpenses { get; set; }
        public int Profits 
        {
            get 
            {
                return TotalRevenue - TotalExpenses;
            }
        }
        public string DayBudduyFuture { get; set; } = "Uncertain";
    }
}
