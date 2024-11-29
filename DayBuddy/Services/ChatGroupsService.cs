using DayBuddy.Models;
using DayBuddy.Settings;
using Microsoft.AspNetCore.Identity;
using MongoDB.Driver;
using MongoDbGenericRepository.Attributes;

namespace DayBuddy.Services
{
    //might need improvement if I want to add a friends list and real time messaging for friends
    //this needs to be scouped not singleton
    public class ChatGroupsService
    {
        private readonly IMongoCollection<BuddyChatGroup> groupsCollection;
        private readonly BuddyGroupCacheService cacheService;
        private readonly UserManager<DayBuddyUser> userManager;
        public ChatGroupsService(IMongoClient mongoClient, MongoDbConfig config, BuddyGroupCacheService cacheService, UserManager<DayBuddyUser> userManager)
        {
            var database = mongoClient.GetDatabase(config.Name);
            var collectionNameAttribute = Attribute.GetCustomAttribute(typeof(BuddyChatGroup), typeof(CollectionNameAttribute)) as CollectionNameAttribute;
            string collectionName = collectionNameAttribute?.Name ?? "ActiveChats";
            groupsCollection = database.GetCollection<BuddyChatGroup>(collectionName);
            this.cacheService = cacheService;
            this.userManager = userManager;
        }

        public async Task ConnectUsers(DayBuddyUser user1, DayBuddyUser user2)
        {
            BuddyChatGroup chatLobby = new([user1.Id, user2.Id]);
            await AddGroupAsync(chatLobby);
            user1.BuddyChatLobbyID = chatLobby.Id; 
            user1.IsAvailable = false;
            user2.BuddyChatLobbyID = chatLobby.Id;
            user2.IsAvailable = false;
            await userManager.UpdateAsync(user1);
            await userManager.UpdateAsync(user2);

            cacheService.AddUser(user1.Id.ToString(),user1.BuddyChatLobbyID.ToString());
            cacheService.AddUser(user2.Id.ToString(), user2.BuddyChatLobbyID.ToString());
        }

        public async Task<List<BuddyChatGroup>> GetActiveGroupsAsync()
        {
            var filter = Builders<BuddyChatGroup>.Filter.Empty;
            return await groupsCollection.Find(filter).ToListAsync();
        }

        public async Task UpdateUsersInGroup(Guid groupId, Guid[] users)
        {

        }

        public async Task RemoveGroupAsync(Guid groupId)
        {
            var filter = Builders<BuddyChatGroup>.Filter.Eq(g => g.Id, groupId);
            await groupsCollection.DeleteOneAsync(filter);
        }

        public async Task AddGroupAsync(BuddyChatGroup group)
        {
            await groupsCollection.InsertOneAsync(group);
        }
    }
}
