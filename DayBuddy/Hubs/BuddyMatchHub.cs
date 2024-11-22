using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace DayBuddy.Hubs
{
    [Authorize]
    public class BuddyMatchHub : Hub
    {
        //when users join, check if he is in a room in the db, if yes, connect it to that room, if not, then add it to a list of active users
        //Maybe also add a Dictionary for chaching
        //Maybe also in the MessagesService cach messages and add them in bulk
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
    }
}
