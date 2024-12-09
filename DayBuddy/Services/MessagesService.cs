using DayBuddy.Models;
using DayBuddy.Services.Caches;
using DayBuddy.Settings;
using MongoDB.Driver;
using MongoDbGenericRepository.Attributes;
using System.Text.RegularExpressions;

namespace DayBuddy.Services
{
    public class MessagesService
    {
        private readonly IMongoCollection<BuddyMessage> _messagesCollection;
        //injected with DI, used to run code when the app is closing
        private readonly IHostApplicationLifetime applicationLifetime;
        private readonly MessagesCacheService messagesCacheService;
        private int WriteToDbThershold = 100;
        public MessagesService(IMongoClient mongoClient, MongoDbConfig config, IHostApplicationLifetime applicationLifetime, MessagesCacheService messagesCacheService)
        {
            this.applicationLifetime = applicationLifetime;
            var database = mongoClient.GetDatabase(config.Name);
            var collectionNameAttribute = Attribute.GetCustomAttribute(typeof(BuddyMessage), typeof(CollectionNameAttribute)) as CollectionNameAttribute;
            string collectionName = collectionNameAttribute?.Name ?? "Messages";
            _messagesCollection = database.GetCollection<BuddyMessage>(collectionName);
            applicationLifetime.ApplicationStopping.Register(OnApplicationShutdown);
            this.messagesCacheService = messagesCacheService;
        }

        private void OnApplicationShutdown()
        {
            Task.Run(SaveAllMessages);
        }

        public async Task<BuddyMessage[]> GetMessageInGroup(Guid groupId,int offset, int amount)
        {
            return [];
        }

        public async Task DeleteMesagesInGroup(Guid groupID)
        {
            var filter = Builders<BuddyMessage>.Filter.Eq(m => m.ChatGroupId, groupID);
            await _messagesCollection.DeleteManyAsync(filter);
        }

        public async Task CreateMessageAsync(BuddyMessage message)
        {
            messagesCacheService.InsertMessage(message.ChatGroupId, message);
            if(messagesCacheService.GetCacheSize(message.ChatGroupId) >= WriteToDbThershold)
            {
                await SaveCacheMessages(message.ChatGroupId);
            }
        }

        private async Task SaveAllMessages()
        {
            await _messagesCollection.InsertManyAsync(messagesCacheService.GetAllCache());
        }

        private async Task SaveCacheMessages(Guid groupId)
        {
            if (messagesCacheService.GetCacheSize(groupId) == 0) return;
            await _messagesCollection.InsertManyAsync(messagesCacheService.GetGroupCache(groupId));
            messagesCacheService.ClearCache(groupId);
            Console.WriteLine("SAVING MESSAGES");
        }
    }
}
