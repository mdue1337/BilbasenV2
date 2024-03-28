using Microsoft.AspNetCore.SignalR;
using MVCtest3d.Database;
using MVCtest3d.Hubs.Model;
using System.Linq;

namespace MVCtest3d.Hubs
{
    public class ChatHub : Hub
    {
        private readonly DatabaseConnection _db;
        public static Dictionary<string, bool> ConnectedIdsChats = new();

        public ChatHub(DatabaseConnection db)
        {
            _db = db;
        }

        // https://stackoverflow.com/questions/13514259/get-number-of-listeners-clients-connected-to-signalr-hub
        // https://learn.microsoft.com/en-us/aspnet/core/tutorials/signalr?view=aspnetcore-8.0&tabs=visual-studio
        // https://stackoverflow.com/questions/17309745/how-to-join-a-group-using-signalr

        public override Task OnConnectedAsync()
        {
            ConnectedIdsChats.Add(Context.ConnectionId, false);
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            ConnectedIdsChats.Remove(Context.ConnectionId);
            return base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(string groupName, int userId, string message)
        {
            _db.ChatSendMessage(int.Parse(groupName), userId, message);
            await Clients.Group(groupName).SendAsync("ReceiveMessage", userId, message);
        }

        public async Task DisplayMessage(int userId, string message)
        {
            if (!ConnectedIdsChats[Context.ConnectionId])
            {
                await Clients.Client(Context.ConnectionId).SendAsync("ReceiveMessage", userId, message);
            }
        }

        public async Task JoinGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        public string GetChatName(int[] users)
        {
            return _db.ConnectChatRoomId(users[0], users[1]).ToString();
        }

        public async Task GetChatHistory(string roomId)
        {
            List<ChatMessageModel> chats = _db.GetChatMessage(int.Parse(roomId));

            if (chats.Count == 0)
            {
                return;
            }

            foreach (ChatMessageModel chat in chats)
            {
                await DisplayMessage(chat.SenderId, chat.Message);
            }

            ConnectedIdsChats[Context.ConnectionId] = true;
        }
    }
}