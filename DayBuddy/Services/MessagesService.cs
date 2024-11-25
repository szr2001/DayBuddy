using DayBuddy.Models;
using DayBuddy.Settings;
using Humanizer;
using MongoDB.Driver;
using MongoDbGenericRepository.Attributes;
using System.Runtime.Intrinsics.X86;
using System.Xml.Linq;
using System;

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

        public async Task CreateMessageAsync(BuddyMessage message)
        {
            await _messagesCollection.InsertOneAsync(message);
        }
    }
}
