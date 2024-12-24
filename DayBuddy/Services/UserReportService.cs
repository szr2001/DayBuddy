using DayBuddy.Models;
using DayBuddy.Settings;
using Microsoft.AspNetCore.Identity;
using MongoDB.Driver;
using MongoDbGenericRepository.Attributes;

namespace DayBuddy.Services
{
    public class UserReportService
    {
        private readonly IMongoCollection<UserReport> reportsCollection;
        public UserReportService(IMongoClient mongoClient, MongoDbConfig mongoDbConfig)
        {
            var database = mongoClient.GetDatabase(mongoDbConfig.Name);
            var collectionNameAttribute = Attribute.GetCustomAttribute(typeof(UserReport), typeof(CollectionNameAttribute)) as CollectionNameAttribute;
            string collectionName = collectionNameAttribute!.Name!;

            reportsCollection = database.GetCollection<UserReport>(collectionName);
        }

        public async Task InsertReport(UserReport report)
        {
            await reportsCollection.InsertOneAsync(report);
        }

        public async Task DeleteUserReports(DayBuddyUser user)
        {
            var filter = Builders<UserReport>.Filter.Eq(u => u.ReportedUserId, user.Id);

            await reportsCollection.DeleteManyAsync(filter);
        }
    }
}
