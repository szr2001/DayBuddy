using DayBuddy.Models;
using DayBuddy.Services.Caches;
using DayBuddy.Settings;
using Microsoft.AspNetCore.Identity;
using MongoDB.Driver;
using MongoDbGenericRepository.Attributes;

namespace DayBuddy.Services
{
    public class MessagesService
    {
        private readonly IMongoCollection<BuddyMessage> messagesCollection;
        private readonly MessagesCacheService messagesCacheService;
        private readonly UserManager<DayBuddyUser> userManager;
        private readonly int WriteToDbThershold = 5;
        public MessagesService(IMongoClient mongoClient, MongoDbConfig nongoConfig, MessagesCacheService messagesCacheService, UserManager<DayBuddyUser> userManager)
        {
            this.messagesCacheService = messagesCacheService;
            this.userManager = userManager;

            var database = mongoClient.GetDatabase(nongoConfig.Name);
            var collectionNameAttribute = Attribute.GetCustomAttribute(typeof(BuddyMessage), typeof(CollectionNameAttribute)) as CollectionNameAttribute;
            string collectionName = collectionNameAttribute!.Name!;
            messagesCollection = database.GetCollection<BuddyMessage>(collectionName);
        }

        public async Task<List<GroupMessage>> GetGroupMessageInGroupAsync(Guid groupId, DayBuddyUser authUser, int offset, int amount)
        {
            List<GroupMessage> groupMessages = new();

            List<BuddyMessage> Messages = await GetBuddyMessageInGroupAsync
                (
                    authUser.BuddyChatGroupID,
                    offset,
                    amount
                );

            foreach (BuddyMessage message in Messages)
            {
                GroupMessage groupMessage = new(message.SenderId.ToString(), message.Message);
                groupMessages.Add(groupMessage);
            }
            return groupMessages;
        }

        public async Task<List<BuddyMessage>> GetBuddyMessageInGroupAsync(Guid groupId, int offset, int amount)
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
                var filter = Builders<BuddyMessage>.Filter.Eq(m => m.ChatGroupId, groupId);
                int remainingOffest = cacheSize > offset ? 0 : offset - cacheSize;

                var restMessages = await messagesCollection.Find(filter)
                                .Sort(Builders<BuddyMessage>.Sort.Descending(m => m.CreatedDate))
                                .Skip(remainingOffest)
                                .Limit(remainingAmount)
                                .ToListAsync();

                messages.AddRange(restMessages);
            }

            return messages.OrderBy(m => m.CreatedDate).ToList();
        }

        public async Task<int>GetMessagesCount()
        {
            var filter = Builders<BuddyMessage>.Filter.Empty;
            return (int)await messagesCollection.CountDocumentsAsync(filter);
        }

        public async Task DeleteMesagesInGroupAsync(Guid groupID)
        {
            var filter = Builders<BuddyMessage>.Filter.Eq(m => m.ChatGroupId, groupID);
            await messagesCollection.DeleteManyAsync(filter);
        }

        public async Task InsertMessageAsync(BuddyMessage message)
        {
            await messagesCollection.InsertOneAsync(message);
        }

        public async Task InsertMessagesAsync(List<BuddyMessage> messages)
        {
            if(messages.Count == 0) return;
            await messagesCollection.InsertManyAsync(messages);
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
            await messagesCollection.InsertManyAsync(messages);
        }
    }
}
