using DayBuddy.Models;
using DayBuddy.Settings;
using MongoDB.Driver;
using MongoDbGenericRepository.Attributes;

namespace DayBuddy.Services
{
    public class ChatLobbysService
    {
        private readonly IMongoCollection<BuddyChatLobby> _lobbyCollection;

        public ChatLobbysService(IMongoClient mongoClient, MongoDbConfig config)
        {
            var database = mongoClient.GetDatabase(config.Name);
            var collectionNameAttribute = Attribute.GetCustomAttribute(typeof(BuddyMessage), typeof(CollectionNameAttribute)) as CollectionNameAttribute;
            string collectionName = collectionNameAttribute?.Name ?? "ActiveChats";
            _lobbyCollection = database.GetCollection<BuddyChatLobby>(collectionName);
        }

        public async Task CreateLobbyAsync(BuddyChatLobby lobby)
        {
            await _lobbyCollection.InsertOneAsync(lobby);
        }
    }
}
