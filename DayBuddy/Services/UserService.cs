using MongoDB.Driver;
using DayBuddy.Models;
using DayBuddy.Settings;
using MongoDbGenericRepository.Attributes;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using Microsoft.AspNetCore.Identity;

namespace DayBuddy.Services
{
    public class UserService
    {
        private readonly IMongoCollection<DayBuddyUser> usersCollection;
        private readonly IConfiguration config;
        private readonly TimeSpan FindBuddyCooldown;
        public UserService(IMongoClient mongoClient, MongoDbConfig mongoDbConfig, IConfiguration config)
        {
            this.config = config;
            var database = mongoClient.GetDatabase(mongoDbConfig.Name);
            var collectionNameAttribute = Attribute.GetCustomAttribute(typeof(DayBuddyUser), typeof(CollectionNameAttribute)) as CollectionNameAttribute;
            string collectionName = collectionNameAttribute!.Name!;

            usersCollection = database.GetCollection<DayBuddyUser>(collectionName);
            //write the whole timespan inside the appsettings

            int hoursCooldown = config.GetValue<int>("FindBuddyCooldownHours");
            FindBuddyCooldown = new(hoursCooldown, 0, 0);
        }

        public async Task<int> GetUsersCount()
        {
            var filter = Builders<DayBuddyUser>.Filter.Empty;
            return (int)await usersCollection.CountDocumentsAsync(filter);
        }

        public async Task<int> GetActiveUsersCount()
        {
            var oneDayAgo = DateTime.UtcNow.AddDays(-1);
            var filter = Builders<DayBuddyUser>.Filter.Gte(u => u.LastTimeOnline, oneDayAgo);
            return (int)await usersCollection.CountDocumentsAsync(filter);
        }

        public async Task<int> GetPremiumUsersCount()
        {
            var filter = Builders<DayBuddyUser>.Filter.Gte(u => u.PremiumExpiryDate, DateTime.UtcNow);
            return (int)await usersCollection.CountDocumentsAsync(filter);
        }

        public bool IsPremiumUser(DayBuddyUser user)
        {
            if(user.PremiumExpiryDate == DateTime.MinValue) return false;

            return user.PremiumExpiryDate > DateTime.UtcNow;
        }

        public bool IsUserOnBuddySearchCooldown(DayBuddyUser user)
        {
            if(user.MatchedWithBuddy == DateTime.MinValue) return false;

            TimeSpan time = DateTime.UtcNow - user.MatchedWithBuddy;

            return time < FindBuddyCooldown;
        }

        public TimeSpan GetUserPremiumDurationLeft(DayBuddyUser user)
        {
            if (user.PremiumExpiryDate == DateTime.MinValue) return TimeSpan.Zero;

            TimeSpan time =  user.PremiumExpiryDate - DateTime.UtcNow;

            return time < TimeSpan.Zero ? TimeSpan.Zero : time;
        }

        public TimeSpan GetUserBuddySearchCooldown(DayBuddyUser user)
        {
            if(user.MatchedWithBuddy == DateTime.MinValue) return TimeSpan.Zero;

            TimeSpan time = DateTime.UtcNow - user.MatchedWithBuddy;

            TimeSpan cooldownLeft = FindBuddyCooldown - time;

            return cooldownLeft < TimeSpan.Zero ? TimeSpan.Zero : cooldownLeft;
        }

        public UserProfile GetUserProfile(DayBuddyUser user)
        {
            UserProfile prfile = new()
            {
                Name = user.UserName,
                Sexuality = user.Sexuality,
                Age = user.Age,
                Interests = user.Interests,
                Gender = user.Gender,
                Country = user.Country,
                City = user.City,
                Score = user.Score,
                Premium = IsPremiumUser(user)
            };
            return prfile;
        }

        public async Task<DayBuddyUser?> GetBuddyMatchForProfileAsync(DayBuddyUser selfUser)
        {
            var oneDayAgo = DateTime.UtcNow.AddDays(-1);

            var filter = Builders<DayBuddyUser>.Filter.And(
                Builders<DayBuddyUser>.Filter.Eq(u => u.IsAvailable, true),
                Builders<DayBuddyUser>.Filter.Ne(u => u.Id, selfUser.Id),
                Builders<DayBuddyUser>.Filter.Not(Builders<DayBuddyUser>.Filter.Where(u => u.ReportedUsers.Contains(selfUser.Id))),
                Builders<DayBuddyUser>.Filter.Not(Builders<DayBuddyUser>.Filter.Where(u => selfUser.ReportedUsers.Contains(u.Id))),
                Builders<DayBuddyUser>.Filter.Gte(u => u.LastTimeOnline, oneDayAgo)
            );

            var aggregation = usersCollection.Aggregate()
                .Match(filter)
                .Project(new BsonDocument
                {
            { "User", "$$ROOT" },
            { "MatchScore", new BsonDocument("$add", new BsonArray
                {
                    new BsonDocument("$cond", new BsonArray
                    {
                        new BsonDocument("$and", new BsonArray
                        {
                            new BsonDocument("$gte", new BsonArray { "$Age", selfUser.AgeRange[0] }),
                            new BsonDocument("$lte", new BsonArray { "$Age", selfUser.AgeRange[1] })
                        }),
                        1,
                        0
                    }),
                    new BsonDocument("$size", new BsonDocument("$setIntersection", new BsonArray
                    {
                        "$Interests",
                        new BsonArray(selfUser.Interests)
                    })),
                    new BsonDocument("$cond", new BsonArray
                    {
                        new BsonDocument("$eq", new BsonArray { "$Country", selfUser.Country }),
                        3,
                        0
                    }),
                    new BsonDocument("$cond", new BsonArray
                    {
                        new BsonDocument("$eq", new BsonArray { "$Sexuality", selfUser.Sexuality }),
                        1,
                        0
                    })
                })
            }
                })
                .Sort(new BsonDocument("MatchScore", -1))
                .Limit(1);

            var result = await aggregation.FirstOrDefaultAsync();
            return result != null ? BsonSerializer.Deserialize<DayBuddyUser>(result["User"].AsBsonDocument) : null;
        }

        public async Task<DayBuddyUser?> GetUserByChatLobbyIdAsync(string lobbyId)
        {
            var filter = Builders<DayBuddyUser>.Filter.Eq(u => u.BuddyChatGroupID, Guid.Parse(lobbyId));
            var user = await usersCollection.Find(filter).FirstOrDefaultAsync();
            return user;
        }
    }
}
