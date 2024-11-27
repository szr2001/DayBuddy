
namespace DayBuddy.BackgroundServices
{
    /// <summary>
    /// Populate the BuddyGroupCacheService for storing data from db in memory ram for fast access
    /// </summary>
    public class GroupCachePopulationService : IHostedService
    {
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
