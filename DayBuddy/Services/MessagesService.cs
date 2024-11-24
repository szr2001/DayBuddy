using DayBuddy.Models;
using DayBuddy.Settings;
using MongoDB.Driver;

namespace DayBuddy.Services
{
    public class MessagesService
    {
        private readonly IMongoCollection<BuddyMessage> _messagesCollection;

        public MessagesService(IMongoClient mongoClient, MongoDbConfig config)
        {
            var database = mongoClient.GetDatabase(config.Name);
            _messagesCollection = database.GetCollection<BuddyMessage>("Messages");
        }

        public async Task CreateMessageAsync(BuddyMessage message)
        {
            await _messagesCollection.InsertOneAsync(message);
        }
    }
}
