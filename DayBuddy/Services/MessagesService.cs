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
        private readonly IConfiguration config;
        private readonly int CacheMessagesWriteToDbThershold;
        private readonly int MaxMessagesInGroup;
        public MessagesService(IMongoClient mongoClient, MongoDbConfig nongoConfig, MessagesCacheService messagesCacheService, UserManager<DayBuddyUser> userManager, IConfiguration config)
        {
            this.messagesCacheService = messagesCacheService;
            this.userManager = userManager;
            this.config = config;

            CacheMessagesWriteToDbThershold = config.GetSection("CacheMessagesWriteToDbThershold").Get<int>();
            MaxMessagesInGroup = config.GetSection("MaxMessagesInGroup").Get<int>();

            var database = mongoClient.GetDatabase(nongoConfig.Name);
            var collectionNameAttribute = Attribute.GetCustomAttribute(typeof(BuddyMessage), typeof(CollectionNameAttribute)) as CollectionNameAttribute;
            string collectionName = collectionNameAttribute!.Name!;
            messagesCollection = database.GetCollection<BuddyMessage>(collectionName);
        }

        public async Task<int> GetGroupMessagesCountAsync(Guid groupId)
        {
            var filter = Builders<BuddyMessage>.Filter.Eq(m => m.ChatGroupId, groupId);
            return (int)await messagesCollection.CountDocumentsAsync(filter);
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
            int cacheSize = messagesCacheService.GetGroupCacheSize(groupId);

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
            if(messagesCacheService.GetGroupCacheSize(message.ChatGroupId) >= CacheMessagesWriteToDbThershold)
            {
                await InsertCacheMessagesAsync(messagesCacheService.GetGroupCache(message.ChatGroupId));
                messagesCacheService.MarkCacheAsInsertedIntoDb(message.ChatGroupId);
                await DeleteHalfOfMessagesInGroup(message.ChatGroupId);
            }
            Console.WriteLine($"Total Group Messages {messagesCacheService.GetGroupDbMessageCount(message.ChatGroupId)}");
            Console.WriteLine($"Total Group Cache Messages {messagesCacheService.GetGroupCacheSize(message.ChatGroupId)}");
        }

        private async Task DeleteHalfOfMessagesInGroup(Guid groupId)
        {
            int groupMessagesCount = messagesCacheService.GetGroupDbMessageCount(groupId);

            if (groupMessagesCount > MaxMessagesInGroup)
            {
                Console.WriteLine("Messages Threshold meet");
                int groupRemoveMessagesCount = (int)(groupMessagesCount * 0.6);

                //filter based on groupID and sort based on creation date
                var filter = Builders<BuddyMessage>.Filter.Eq(mess => mess.ChatGroupId, groupId);
                var sort = Builders<BuddyMessage>.Sort.Ascending(msg => msg.CreatedDate);

                //find the messages that fit the query and limit to the removeMesscount
                var messagesToDelete = await messagesCollection
                    .Find(filter)
                    .Sort(sort)
                    .Limit(groupRemoveMessagesCount)
                    .Project(msg => msg.Id)
                    .ToListAsync();

                //create a filter to check if the message Id is inside the list of messages id to delete
                var deleteFilter = Builders<BuddyMessage>.Filter.In(msg => msg.Id, messagesToDelete);
                var result = await messagesCollection.DeleteManyAsync(deleteFilter);
                Console.WriteLine($"Deleted: {result.DeletedCount}");
                messagesCacheService.SetGroupMessageCount(groupId, groupRemoveMessagesCount);

            }
        }

        private async Task InsertCacheMessagesAsync(List<BuddyMessage> messages)
        {
            if (messages.Count == 0) return;
            await messagesCollection.InsertManyAsync(messages);
        }
    }
}
