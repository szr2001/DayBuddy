using DayBuddy.Models;
using DayBuddy.Settings;
using MongoDB.Driver;

namespace DayBuddy.Services
{
    public class ChatLobbysService
    {
        private readonly IMongoCollection<BuddyChatLobby> _messagesCollection;

        public ChatLobbysService(IMongoClient mongoClient, MongoDbConfig config)
        {
            var database = mongoClient.GetDatabase(config.Name);
            _messagesCollection = database.GetCollection<BuddyChatLobby>("ActiveChats");
        }

        public async Task CreateMessageAsync(BuddyChatLobby message)
        {
            await _messagesCollection.InsertOneAsync(message);
        }
    }
}
