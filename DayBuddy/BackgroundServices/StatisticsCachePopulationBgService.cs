using DayBuddy.Models;
using DayBuddy.Services;
using DayBuddy.Services.Caches;

namespace DayBuddy.BackgroundServices
{
    public class StatisticsCachePopulationBgService : IHostedService
    {
        private readonly IServiceScopeFactory scopeFactory;
        private readonly StatisticsCache statisticsCache;
        private DayBuddyStats? stats;
        public StatisticsCachePopulationBgService(StatisticsCache statisticsCache, IServiceScopeFactory scopeFactory)
        {
            this.statisticsCache = statisticsCache;
            this.scopeFactory = scopeFactory;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using (var scope = scopeFactory.CreateScope())
            {
                UserService userService = scope.ServiceProvider.GetRequiredService<UserService>();
                UserReportService userReportService = scope.ServiceProvider.GetRequiredService<UserReportService>();
                StatsService statsService = scope.ServiceProvider.GetRequiredService<StatsService>();

                stats = await statsService.RetrieveStatsAsync();

                statisticsCache.TotalUsers = await userService.GetUsersCount();
                statisticsCache.ActiveUsers = await userService.GetActiveUsersCount();
                statisticsCache.PremiumUsers = await userService.GetPremiumUsersCount();
                statisticsCache.TotalReports = await userReportService.GetReportsCount();
                statisticsCache.ExpectedExpenses = stats.ExpectedExpenses;
                statisticsCache.TotalExpenses = stats.TotalExpenses;
                statisticsCache.TotalRevenue = stats.TotalRevenue;
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (stats == null) return;

            using (var scope = scopeFactory.CreateScope())
            {
                StatsService statsService = scope.ServiceProvider.GetRequiredService<StatsService>();

                stats.ExpectedExpenses = statisticsCache.ExpectedExpenses;
                stats.TotalExpenses = statisticsCache.TotalExpenses;
                stats.TotalRevenue = statisticsCache.TotalRevenue;
                stats.LastTimeUpdated = DateTime.UtcNow;

                await statsService.UpdateStatsAsync(stats);
            }
        }
    }
}
