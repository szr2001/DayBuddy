using DayBuddy.Services;
using DayBuddy.Services.Caches;

namespace DayBuddy.BackgroundServices
{
    public class StatisticsCachePopulationBgService : IHostedService
    {
        private readonly IServiceScopeFactory scopeFactory;
        private readonly StatisticsCache statisticsCache;
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

                statisticsCache.TotalUsers = await userService.GetUsersCount();
                statisticsCache.ActiveUsers = await userService.GetActiveUsersCount();
                statisticsCache.PremiumUsers = await userService.GetPremiumUsersCount();
                statisticsCache.TotalReports = await userReportService.GetReportsCount();
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
