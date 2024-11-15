using AspNetCore.Identity.MongoDbCore;
using AspNetCore.Identity.MongoDbCore.Models;
using MongoDbGenericRepository.Attributes;

namespace DayBuddy.Models
{
    [CollectionName("Users")]
    public class DayBuddyUser : MongoIdentityUser<Guid>
    {

    }
}
