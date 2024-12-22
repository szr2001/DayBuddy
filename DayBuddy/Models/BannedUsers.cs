using MongoDbGenericRepository.Attributes;

namespace DayBuddy.Models
{
    [CollectionName("BannedUsers")]
    public class BannedUsers
    {
        public Guid Id;
        public string? email;

        public BannedUsers(Guid id)
        {
            Id = id;
        }
    }
}
