﻿using MongoDB.Driver;
using DayBuddy.Models;
using DayBuddy.Settings;
using MongoDbGenericRepository.Attributes;

namespace DayBuddy.Services
{
    public class UserService
    {
        private readonly IMongoCollection<DayBuddyUser> usersCollection;

        public UserService(IMongoClient mongoClient, MongoDbConfig mongoDbConfig)
        {
            var database = mongoClient.GetDatabase(mongoDbConfig.Name);
            var collectionNameAttribute = Attribute.GetCustomAttribute(typeof(DayBuddyUser), typeof(CollectionNameAttribute)) as CollectionNameAttribute;
            string collectionName = collectionNameAttribute?.Name ?? "Users";

            usersCollection = database.GetCollection<DayBuddyUser>(collectionName);
        }

        public async Task<DayBuddyUser?> GetRandomUserByAgeAsync(int minAge, int maxAge)
        {
            var random = new Random();
            var filter = Builders<DayBuddyUser>.Filter.Gte(u => u.Age, minAge) &
                         Builders<DayBuddyUser>.Filter.Lte(u => u.Age, maxAge);

            var users = await usersCollection.Find(filter).ToListAsync();
            if (users.Count == 0) return null;

            return users[random.Next(users.Count)];
        }

        public async Task<List<DayBuddyUser>> GetUsersByInterestsAsync(string interest)
        {
            var filter = Builders<DayBuddyUser>.Filter.AnyEq(u => u.Interests, interest);
            var users = await usersCollection.Find(filter).ToListAsync();
            return users;
        }

        public async Task<DayBuddyUser?> GetRndAvailableUserAsync(DayBuddyUser ignoreUser)
        {
            var random = new Random();
            var filter = Builders<DayBuddyUser>.Filter.Eq(u => u.IsAvailable, true) &
                             Builders<DayBuddyUser>.Filter.Ne(u => u.Id, ignoreUser.Id);
            var users = await usersCollection.Find(filter).ToListAsync();
            Console.WriteLine(users.Count);
            if (users.Count == 0) return null;

            return users[random.Next(users.Count)];
        }

        public async Task<DayBuddyUser?> GetUserByChatLobbyIdAsync(string lobbyId)
        {
            var filter = Builders<DayBuddyUser>.Filter.Eq(u => u.BuddyChatLobbyID, Guid.Parse(lobbyId));
            var user = await usersCollection.Find(filter).FirstOrDefaultAsync();
            return user;
        }
    }
}
