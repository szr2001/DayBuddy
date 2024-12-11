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
        private readonly IMongoCollection<BuddyMessage> _messagesCollection;
        private readonly MessagesCacheService messagesCacheService;
        private readonly UserManager<DayBuddyUser> userManager;
        private int WriteToDbThershold = 5;
        public MessagesService(IMongoClient mongoClient, MongoDbConfig config, MessagesCacheService messagesCacheService, UserManager<DayBuddyUser> userManager)
        {
            this.userManager = userManager;
            this.messagesCacheService = messagesCacheService;

            var database = mongoClient.GetDatabase(config.Name);
            var collectionNameAttribute = Attribute.GetCustomAttribute(typeof(BuddyMessage), typeof(CollectionNameAttribute)) as CollectionNameAttribute;
            string collectionName = collectionNameAttribute?.Name ?? "Messages";
            _messagesCollection = database.GetCollection<BuddyMessage>(collectionName);
        }

        //bug here, doesn't seem to get in the correct order
        public async Task<List<GroupMessage>> GetGroupMessageInGroupAsync(Guid groupId, DayBuddyUser authUser, int offset, int amount)
        {
            Dictionary<Guid, DayBuddyUser?> userCache = new()
            {
                { authUser.BuddyChatGroupID, authUser }
            };

            List<GroupMessage> groupMessages = new();

            List<BuddyMessage> Messages = await GetBuddyMessageInGroupAsync
                (
                    authUser.BuddyChatGroupID,
                    offset,
                    amount
                );

            foreach (BuddyMessage message in Messages)
            {
                if (!userCache.ContainsKey(message.SenderId))
                {
                    DayBuddyUser? senderUser = await userManager.FindByIdAsync(message.SenderId.ToString());
                    userCache.Add(message.SenderId, senderUser);
                }
                if (userCache[message.SenderId] == null) continue;

                GroupMessage groupMessage = new(userCache[message.SenderId]!.UserName!, message.Message);
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

                var restMessages = await _messagesCollection.Find(filter)
                                .Sort(Builders<BuddyMessage>.Sort.Descending(m => m.CreatedDate))
                                .Skip(remainingOffest)
                                .Limit(remainingAmount)
                                .ToListAsync();

                messages.AddRange(restMessages);
            }

            return messages.OrderBy(m => m.CreatedDate).ToList();
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
