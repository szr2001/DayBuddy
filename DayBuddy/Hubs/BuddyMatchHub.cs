using DayBuddy.Services;
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
        private readonly UserCacheService userCacheService;
        private readonly ChatLobbysService chatLobbysService;
        private readonly MessagesService messagesService;
        public BuddyMatchHub(UserCacheService userCacheService, ChatLobbysService chatLobbysService, MessagesService messagesService)
        {
            //Context.User get the current user similar to controllers
            this.userCacheService = userCacheService;
            this.chatLobbysService = chatLobbysService;
            this.messagesService = messagesService;
        }

        public override Task OnConnectedAsync()
        {
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            return base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(string user, string message)
        {
            //we can trigger other events and do other things, like for match
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
    }
}
