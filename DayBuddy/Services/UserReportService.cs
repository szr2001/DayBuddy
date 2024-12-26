using DayBuddy.Models;
using DayBuddy.Services.Caches;
using DayBuddy.Settings;
using Microsoft.AspNetCore.Identity;
using MongoDB.Driver;
using MongoDbGenericRepository.Attributes;

namespace DayBuddy.Services
{
    public class UserReportService
    {
        private readonly IMongoCollection<UserReport> reportsCollection;
        private readonly StatisticsCache statisticsCache;
        public UserReportService(IMongoClient mongoClient, MongoDbConfig mongoDbConfig, StatisticsCache statisticsCache)
        {
            var database = mongoClient.GetDatabase(mongoDbConfig.Name);
            var collectionNameAttribute = Attribute.GetCustomAttribute(typeof(UserReport), typeof(CollectionNameAttribute)) as CollectionNameAttribute;
            string collectionName = collectionNameAttribute!.Name!;

            reportsCollection = database.GetCollection<UserReport>(collectionName);
            this.statisticsCache = statisticsCache;
        }

        public async Task<int>GetReportsCount()
        {
            var filter = Builders<UserReport>.Filter.Empty;
            return (int)await reportsCollection.CountDocumentsAsync(filter);
        }

        public async Task InsertReport(UserReport report)
        {
            statisticsCache.TotalReports++;
            await reportsCollection.InsertOneAsync(report);
        }

        public async Task DeleteUserReports(DayBuddyUser user)
        {
            var filter = Builders<UserReport>.Filter.Eq(u => u.ReportedUserId, user.Id);

            var result = await reportsCollection.DeleteManyAsync(filter);

            statisticsCache.TotalReports -= (int)result.DeletedCount;
        }
    }
}
