
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

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
