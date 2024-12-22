using DayBuddy.Models;
using DayBuddy.Settings;
using MongoDB.Driver;
using MongoDbGenericRepository.Attributes;

namespace DayBuddy.Services
{
    public class BannedUsersService
    {
        private readonly IMongoCollection<BannedUsers> bannedUsersCollection;
        public BannedUsersService(IMongoClient mongoClient, MongoDbConfig nongoConfig)
        {
            var database = mongoClient.GetDatabase(nongoConfig.Name);
            var collectionNameAttribute = Attribute.GetCustomAttribute(typeof(BannedUsers), typeof(CollectionNameAttribute)) as CollectionNameAttribute;
            string collectionName = collectionNameAttribute!.Name!;
            bannedUsersCollection = database.GetCollection<BannedUsers>(collectionName);
        }
    }
}
