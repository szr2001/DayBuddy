
using DayBuddy.Models;
using DayBuddy.Services;

namespace DayBuddy.BackgroundServices
{
    /// <summary>
    /// Populate the BuddyGroupCacheService for storing data from db in memory ram for fast access
    /// </summary>
    public class GroupCachePopulationService : IHostedService
    {
        private readonly BuddyGroupCacheService buddyGroupCacheService;
        private readonly ChatGroupsService chatGroupsService;
        public GroupCachePopulationService(BuddyGroupCacheService buddyGroupCacheService, ChatGroupsService chatGroupsService)
        {
            this.buddyGroupCacheService = buddyGroupCacheService;
            this.chatGroupsService = chatGroupsService;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            List<BuddyChatGroup> groups = await chatGroupsService.GetActiveGroupsAsync();

            foreach(BuddyChatGroup group in groups)
            {
                foreach(var user in group.Users)
                {
                    buddyGroupCacheService.AddUser(user.ToString(), group.Id.ToString());
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
