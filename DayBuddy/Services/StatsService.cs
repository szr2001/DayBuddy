using DayBuddy.Models;
using DayBuddy.Settings;
using MongoDB.Driver;
using MongoDbGenericRepository.Attributes;

namespace DayBuddy.Services
{
    public class StatsService
    {
        private readonly IMongoCollection<DayBuddyStats> statsCollection;

        public StatsService(IMongoClient mongoClient, MongoDbConfig mongoDbConfig)
        {
            var database = mongoClient.GetDatabase(mongoDbConfig.Name);
            var collectionNameAttribute = Attribute.GetCustomAttribute(typeof(DayBuddyStats), typeof(CollectionNameAttribute)) as CollectionNameAttribute;
            string collectionName = collectionNameAttribute!.Name!;

            statsCollection = database.GetCollection<DayBuddyStats>(collectionName);
        }

        public async Task<DayBuddyStats> RetrieveStatsAsync()
        {
            var filter = Builders<DayBuddyStats>.Filter.Empty;

           return await statsCollection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task UpdateStatsAsync(DayBuddyStats newStats)
        {
            var filter = Builders<DayBuddyStats>.Filter.Empty;

            //replaces the id too, bad
            await statsCollection.ReplaceOneAsync
                (
                    filter,
                    newStats,
                    new ReplaceOptions {IsUpsert = true }
                );
        }
    }
}
