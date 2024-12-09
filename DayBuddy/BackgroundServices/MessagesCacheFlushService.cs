
using DayBuddy.Models;
using DayBuddy.Services;
using DayBuddy.Services.Caches;

namespace DayBuddy.BackgroundServices
{
    public class MessagesCacheFlushService : IHostedService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly MessagesCacheService messagesCacheService;
        public MessagesCacheFlushService(MessagesCacheService messagesCacheService, IServiceScopeFactory scopeFactory)
        {
            this.messagesCacheService = messagesCacheService;
            _scopeFactory = scopeFactory;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                MessagesService messagesService = scope.ServiceProvider.GetRequiredService<MessagesService>();
                await messagesService.InsertMessagesAsync(messagesCacheService.GetAllCache());
            }
        }
    }
}
