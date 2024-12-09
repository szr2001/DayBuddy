using DayBuddy.Models;
using DayBuddy.Services.Caches;
using DayBuddy.Settings;
using MongoDB.Driver;
using MongoDbGenericRepository.Attributes;

namespace DayBuddy.Services
{
    public class MessagesService
    {
        private readonly IMongoCollection<BuddyMessage> _messagesCollection;
        private readonly MessagesCacheService messagesCacheService;
        private int WriteToDbThershold = 100;
        public MessagesService(IMongoClient mongoClient, MongoDbConfig config, MessagesCacheService messagesCacheService)
        {
            var database = mongoClient.GetDatabase(config.Name);
            var collectionNameAttribute = Attribute.GetCustomAttribute(typeof(BuddyMessage), typeof(CollectionNameAttribute)) as CollectionNameAttribute;
            string collectionName = collectionNameAttribute?.Name ?? "Messages";
            _messagesCollection = database.GetCollection<BuddyMessage>(collectionName);
            this.messagesCacheService = messagesCacheService;
        }

        //ofset 10 amount 10 cache 5 db 20
        public async Task<List<BuddyMessage>> GetMessageInGroupAsync(Guid groupId, int offset, int amount)
        {
            List<BuddyMessage> messages = new();
            int cacheSize = messagesCacheService.GetCacheSize(groupId);

            if (cacheSize > 0 && cacheSize > offset)
            {
                messages = messagesCacheService.GetGroupCache(groupId)
                            .OrderByDescending(m => m.CreatedDate) 
                            .Skip(offset) 
                            .Take(amount) 
                            .ToList();
            }
            
            int remainingAmount = amount - messages.Count;

            if(remainingAmount > 0)
            {
                var filter = Builders<BuddyMessage>.Filter.Eq(m => m.Id, groupId);
                int remainingOffest = cacheSize > offset ? 0 : offset - cacheSize;

                var restMessages = await _messagesCollection.Find(filter)
                                .Sort(Builders<BuddyMessage>.Sort.Descending(m => m.CreatedDate))
                                .Skip(remainingOffest)
                                .Limit(remainingAmount)
                                .ToListAsync();

                messages.AddRange(restMessages);
            }

            return messages;
        }

        public async Task DeleteMesagesInGroupAsync(Guid groupID)
        {
            var filter = Builders<BuddyMessage>.Filter.Eq(m => m.ChatGroupId, groupID);
            await _messagesCollection.DeleteManyAsync(filter);
        }

        public async Task InsertMessageAsync(BuddyMessage message)
        {
            await _messagesCollection.InsertOneAsync(message);
        }

        public async Task InsertMessagesAsync(List<BuddyMessage> messages)
        {
            if(messages.Count == 0) return;
            await _messagesCollection.InsertManyAsync(messages);
        }

        public async Task CacheMessageAsync(BuddyMessage message)
        {
            messagesCacheService.InsertMessage(message.ChatGroupId, message);
            if(messagesCacheService.GetCacheSize(message.ChatGroupId) >= WriteToDbThershold)
            {
                await InsertCacheMessagesAsync(messagesCacheService.GetGroupCache(message.ChatGroupId));
                messagesCacheService.ClearCache(message.ChatGroupId);
            }
        }

        private async Task InsertCacheMessagesAsync(List<BuddyMessage> messages)
        {
            if (messages.Count == 0) return;
            await _messagesCollection.InsertManyAsync(messages);
        }
    }
}
