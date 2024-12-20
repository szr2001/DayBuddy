
using DayBuddy.Models;
using DayBuddy.Services;
using DayBuddy.Services.Caches;

namespace DayBuddy.BackgroundServices
{
    /// <summary>
    /// Populate the BuddyGroupCacheService for storing data from db in memory ram for fast access
    /// </summary>
    public class GroupCachePopulationService : IHostedService
    {
        private readonly IServiceScopeFactory scopeFactory;
        public GroupCachePopulationService(IServiceScopeFactory scopeFactory)
        {
            this.scopeFactory = scopeFactory;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using (var scope = scopeFactory.CreateScope())
            {
                ChatGroupsService chatGroupsService = scope.ServiceProvider.GetRequiredService<ChatGroupsService>();
                BuddyGroupCacheService buddyGroupCacheService = scope.ServiceProvider.GetRequiredService<BuddyGroupCacheService>();

                List<BuddyChatGroup> groups = await chatGroupsService.GetActiveGroupsAsync();

                foreach (BuddyChatGroup group in groups)
                {
                    foreach (var user in group.Users)
                    {
                        buddyGroupCacheService.AddUser(user.ToString(), group.Id.ToString());
                    }
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
