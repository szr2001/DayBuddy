using DayBuddy.Models;
using DayBuddy.Settings;
using MongoDB.Driver;
using MongoDbGenericRepository.Attributes;

namespace DayBuddy.Services
{
    public class FeedbackService
    {
        private readonly IMongoCollection<Feedback> feedbackCollection;
        public FeedbackService(IMongoClient mongoClient, MongoDbConfig nongoConfig)
        {
            var database = mongoClient.GetDatabase(nongoConfig.Name);
            var collectionNameAttribute = Attribute.GetCustomAttribute(typeof(Feedback), typeof(CollectionNameAttribute)) as CollectionNameAttribute;
            string collectionName = collectionNameAttribute!.Name!;
            feedbackCollection = database.GetCollection<Feedback>(collectionName);
        }

        public async Task InsertAsync(Feedback feedback)
        {
            await feedbackCollection.InsertOneAsync(feedback);
        }

        public async Task DeleteAsync(Guid feedbackId)
        {
            var filter = Builders<Feedback>.Filter.Eq(f => f.Id, feedbackId);
            await feedbackCollection.FindOneAndDeleteAsync(filter);
        }

        public async Task DeleteUserFeedback(Guid userId)
        {
            var filter = Builders<Feedback>.Filter.Eq(f => f.SenderId, userId);

            await feedbackCollection.DeleteManyAsync(filter);
        }
    }
}
