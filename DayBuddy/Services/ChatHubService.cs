using Microsoft.AspNetCore.SignalR;

namespace DayBuddy.Services
{
    public class ChatHubService : Hub
    {
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
    }
}
