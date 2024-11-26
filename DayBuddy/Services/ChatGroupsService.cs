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
            BuddyChatGroup chatLobby = new([user1.Id.ToString(), user2.Id.ToString()]);
            await InsertLobbyAsync(chatLobby);
            user1.BuddyChatLobbyID = chatLobby.Id; 
            user1.IsAvailable = false;
            user2.BuddyChatLobbyID = chatLobby.Id;
            user2.IsAvailable = false;
            await userManager.UpdateAsync(user1);
            await userManager.UpdateAsync(user2);

            cacheService.AddUser(user1.Id.ToString(),user1.BuddyChatLobbyID);
            cacheService.AddUser(user2.Id.ToString(),user2.BuddyChatLobbyID);
            //create group
            //asign users to group
            //save group in db and memory
            //save grou to users
            //set users to not available
        }

        public async Task InsertLobbyAsync(BuddyChatGroup lobby)
        {
            await groupsCollection.InsertOneAsync(lobby);
        }
    }
}
