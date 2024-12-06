using DayBuddy.Models;
using DayBuddy.Settings;
using MongoDB.Driver;
using MongoDbGenericRepository.Attributes;

namespace DayBuddy.Services
{
    public class MessagesService
    {
        private readonly IMongoCollection<BuddyMessage> _messagesCollection;

        public MessagesService(IMongoClient mongoClient, MongoDbConfig config)
        {
            var database = mongoClient.GetDatabase(config.Name);
           var collectionNameAttribute = Attribute.GetCustomAttribute(typeof(BuddyMessage), typeof(CollectionNameAttribute)) as CollectionNameAttribute;
            string collectionName = collectionNameAttribute?.Name ?? "Messages";
            _messagesCollection = database.GetCollection<BuddyMessage>(collectionName);
        }

        public async Task<BuddyMessage[]> GetMessageInGroup(Guid groupId,int offset, int amount)
        {
            return [];
        }

        public async Task DeleteMesagesInGroup(Guid groupID)
        {
            var filter = Builders<BuddyMessage>.Filter.Eq(m => m.ChatLobbyId, groupID);
            await _messagesCollection.DeleteManyAsync(filter);
        }

        public async Task CreateMessageAsync(BuddyMessage message)
        {
            await _messagesCollection.InsertOneAsync(message);
        }
    }
}
