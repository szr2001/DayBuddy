using DayBuddy.Hubs;
using DayBuddy.Models;
using DayBuddy.Services.Caches;
using DayBuddy.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver;
using MongoDbGenericRepository.Attributes;
using System.Text.RegularExpressions;

namespace DayBuddy.Services
{
    //might need improvement if I want to add a friends list and real time messaging for friends
    public class ChatGroupsService
    {
        private readonly IHubContext<BuddyMatchHub> buddyMathHubContext;
        private readonly IMongoCollection<BuddyChatGroup> groupsCollection;
        private readonly BuddyGroupCacheService buddyGroupCacheService;
        private readonly UserManager<DayBuddyUser> userManager;
        private readonly MessagesService messagesService;
        private readonly MessagesCacheService messagesCacheService;
        public ChatGroupsService(IMongoClient mongoClient, MongoDbConfig config, BuddyGroupCacheService cacheService, UserManager<DayBuddyUser> userManager, MessagesService messagesService, IHubContext<BuddyMatchHub> buddyMathHubContext, MessagesCacheService messagesCacheService)
        {
            var database = mongoClient.GetDatabase(config.Name);
            var collectionNameAttribute = Attribute.GetCustomAttribute(typeof(BuddyChatGroup), typeof(CollectionNameAttribute)) as CollectionNameAttribute;
            string collectionName = collectionNameAttribute!.Name!;
            groupsCollection = database.GetCollection<BuddyChatGroup>(collectionName);
            this.buddyGroupCacheService = cacheService;
            this.userManager = userManager;
            this.messagesService = messagesService;
            this.buddyMathHubContext = buddyMathHubContext;
            this.messagesCacheService = messagesCacheService;
        }

        public async Task AddBuddyGroup(DayBuddyUser user1, DayBuddyUser user2)
        {
            BuddyChatGroup chatLobby = new([user1.Id, user2.Id]);
            
            await groupsCollection.InsertOneAsync(chatLobby);

            user1.MatchedWithBuddy = DateTime.UtcNow;
            user1.BuddyChatGroupID = chatLobby.Id;
            user1.IsAvailable = false;

            user2.MatchedWithBuddy = DateTime.UtcNow;
            user2.BuddyChatGroupID = chatLobby.Id;
            user2.IsAvailable = false;

            await userManager.UpdateAsync(user1);
            await userManager.UpdateAsync(user2);

            buddyGroupCacheService.AddUser(user1.Id.ToString(),user1.BuddyChatGroupID.ToString());
            buddyGroupCacheService.AddUser(user2.Id.ToString(), user2.BuddyChatGroupID.ToString());

            await buddyMathHubContext.Clients.User(user1.Id.ToString()).SendAsync("Matched");
            await buddyMathHubContext.Clients.User(user2.Id.ToString()).SendAsync("Matched");
        }

        public async Task<List<BuddyChatGroup>> GetActiveGroupsAsync()
        {
            var filter = Builders<BuddyChatGroup>.Filter.Empty;
            return await groupsCollection.Find(filter).ToListAsync();
        }

        public async Task RemoveBuddyGroup(Guid groupId)
        {
            var filter = Builders<BuddyChatGroup>.Filter.Eq(g => g.Id, groupId);
            BuddyChatGroup? group = await groupsCollection.FindOneAndDeleteAsync(filter);
            await messagesService.DeleteMesagesInGroupAsync(groupId);
            if (group != null)
            {
                DayBuddyUser? user1 = await userManager.FindByIdAsync(group.Users[0].ToString());
                DayBuddyUser? user2 = await userManager.FindByIdAsync(group.Users[1].ToString());

                if (user1 != null)
                {
                    user1.BuddyChatGroupID = Guid.Empty;
                    await userManager.UpdateAsync(user1);
                    await buddyMathHubContext.Clients.User(user1.Id.ToString()).SendAsync("UnMatched");
                }
                if (user2 != null)
                {
                    user2.BuddyChatGroupID = Guid.Empty;
                    await userManager.UpdateAsync(user2);
                    await buddyMathHubContext.Clients.User(user2.Id.ToString()).SendAsync("UnMatched");
                }
                buddyGroupCacheService.RemoveGroup(groupId.ToString());
                messagesCacheService.RemoveCache(groupId);
            }
        }
    }
}
