using AspNetCore.Identity.MongoDbCore.Models;
using MongoDbGenericRepository.Attributes;

namespace DayBuddy.Models
{
    [CollectionName("Roles")]
    public class DayBuddyRole : MongoIdentityRole<Guid>
    {
    }
}
