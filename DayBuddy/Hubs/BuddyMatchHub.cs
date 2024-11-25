using DayBuddy.Models;
using DayBuddy.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Build.Framework;

namespace DayBuddy.Hubs
{
    [Authorize]
    public class BuddyMatchHub : Hub
    {
        //when users join, check if he is in a room in the db, if yes, connect it to that room, if not, then add it to a list of active users
        //Maybe also add a Dictionary for chaching
        //Maybe also in the MessagesService cach messages and add them in bulk
        private readonly UserCacheService userCacheService;
        private readonly UserManager<DayBuddyUser> userManager;
        private readonly ChatLobbysService chatLobbysService;
        private readonly MessagesService messagesService;
        public BuddyMatchHub(UserCacheService userCacheService, ChatLobbysService chatLobbysService, MessagesService messagesService, UserManager<DayBuddyUser> userManager)
        {
            //Context.User get the current user similar to controllers
            this.userCacheService = userCacheService;
            this.chatLobbysService = chatLobbysService;
            this.messagesService = messagesService;
            this.userManager = userManager;
        }

        public override Task OnConnectedAsync()
        {
            string? userId = userManager.GetUserId(Context.User!);
            if(userId != null)
            {
                userCacheService.AddUser(userId);
            }
            Console.WriteLine(userCacheService.ActiveUsers);
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            string? userId = userManager.GetUserId(Context.User!);
            if (userId != null)
            {
                userCacheService.RemoveUser(userId);
            }
            Console.WriteLine(userCacheService.ActiveUsers);
            return base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(string user, string message)
        {
            //we can trigger other events and do other things, like for match
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
    }
}
