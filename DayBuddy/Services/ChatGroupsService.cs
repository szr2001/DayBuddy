﻿using DayBuddy.Hubs;
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
        private readonly BuddyGroupCacheService cacheService;
        private readonly UserManager<DayBuddyUser> userManager;
        private readonly MessagesService messagesService;
        public ChatGroupsService(IMongoClient mongoClient, MongoDbConfig config, BuddyGroupCacheService cacheService, UserManager<DayBuddyUser> userManager, MessagesService messagesService, IHubContext<BuddyMatchHub> buddyMathHubContext)
        {
            var database = mongoClient.GetDatabase(config.Name);
            var collectionNameAttribute = Attribute.GetCustomAttribute(typeof(BuddyChatGroup), typeof(CollectionNameAttribute)) as CollectionNameAttribute;
            string collectionName = collectionNameAttribute?.Name ?? "ActiveChats";
            groupsCollection = database.GetCollection<BuddyChatGroup>(collectionName);
            this.cacheService = cacheService;
            this.userManager = userManager;
            this.messagesService = messagesService;
            this.buddyMathHubContext = buddyMathHubContext;
        }

        public async Task AddBuddyGroup(DayBuddyUser user1, DayBuddyUser user2)
        {
            BuddyChatGroup chatLobby = new([user1.Id, user2.Id]);
            
            await groupsCollection.InsertOneAsync(chatLobby);

            user1.MatchedWithBuddy = DateTime.UtcNow;
            user1.BuddyChatLobbyID = chatLobby.Id;
            user1.IsAvailable = false;

            user2.MatchedWithBuddy = DateTime.UtcNow;
            user2.BuddyChatLobbyID = chatLobby.Id;
            user2.IsAvailable = false;

            await userManager.UpdateAsync(user1);
            await userManager.UpdateAsync(user2);

            cacheService.AddUser(user1.Id.ToString(),user1.BuddyChatLobbyID.ToString());
            cacheService.AddUser(user2.Id.ToString(), user2.BuddyChatLobbyID.ToString());

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
            await messagesService.DeleteMesagesInGroup(groupId);
            if (group != null)
            {
                DayBuddyUser? user1 = await userManager.FindByIdAsync(group.Users[0].ToString());
                DayBuddyUser? user2 = await userManager.FindByIdAsync(group.Users[1].ToString());

                if (user1 != null)
                {
                    user1.BuddyChatLobbyID = Guid.Empty;
                    await userManager.UpdateAsync(user1);
                    await buddyMathHubContext.Clients.User(user1.Id.ToString()).SendAsync("UnMatched");
                }
                if (user2 != null)
                {
                    user2.BuddyChatLobbyID = Guid.Empty;
                    await userManager.UpdateAsync(user2);
                    await buddyMathHubContext.Clients.User(user2.Id.ToString()).SendAsync("UnMatched");
                }
                cacheService.RemoveGroup(groupId.ToString());
            }
        }
    }
}
