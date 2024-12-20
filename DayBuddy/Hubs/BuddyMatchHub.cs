using DayBuddy.Models;
using DayBuddy.Services;
using DayBuddy.Services.Caches;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Build.Framework;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace DayBuddy.Hubs
{
    //use the js chat class inside the searchBody view and
    //use a OnMatched event to refresh the page
    //use the same hub
    [Authorize]
    public class BuddyMatchHub : Hub
    {
        //when users join, check if he is in a room in the db, if yes, connect it to that room, if not, then add it to a list of active users
        //Maybe also add a Dictionary for chaching
        //Maybe also in the MessagesService cach messages and add them in bulk
        private readonly BuddyGroupCacheService groupsCacheService;
        private readonly UserManager<DayBuddyUser> userManager;
        private readonly ChatGroupsService chatLobbysService;
        private readonly MessagesService messagesService;
        public BuddyMatchHub(BuddyGroupCacheService groupsCacheService, ChatGroupsService chatLobbysService, MessagesService messagesService, UserManager<DayBuddyUser> userManager)
        {
            //Context.User get the current user similar to controllers
            this.groupsCacheService = groupsCacheService;
            this.chatLobbysService = chatLobbysService;
            this.messagesService = messagesService;
            this.userManager = userManager;
        }
        public override async Task OnConnectedAsync()
        {
            DayBuddyUser? user = await userManager.GetUserAsync(Context.User!);
            if (user == null || user.BuddyChatGroupID == Guid.Empty) return;
            await Groups.AddToGroupAsync(Context.ConnectionId, user.BuddyChatGroupID.ToString());
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            DayBuddyUser? user = await userManager.GetUserAsync(Context.User!);
            if (user == null || user.BuddyChatGroupID == Guid.Empty) return;
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, user.BuddyChatGroupID.ToString());
        }

        public async Task SendMessage(string message)
        {
            string? localUserId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (localUserId == null) return;

            string? groupId = groupsCacheService.GetUserGroup(localUserId);
            if(groupId == null) return;

            BuddyMessage newMessage = new(message, Guid.Parse(localUserId),Guid.Parse(groupId));
            await messagesService.CacheMessageAsync(newMessage);

            await Clients.Group(groupId).SendAsync("ReceiveMessage", localUserId, message);
        }
    }
}
