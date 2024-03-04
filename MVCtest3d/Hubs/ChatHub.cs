using Microsoft.AspNetCore.SignalR;

namespace MVCtest3d.Hubs
{
    public class ChatHub : Hub
    {
        // https://learn.microsoft.com/en-us/aspnet/core/tutorials/signalr?view=aspnetcore-8.0&tabs=visual-studio
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
    }
}
