using Microsoft.AspNetCore.SignalR;

namespace BuddyAssist.API.Hubs
{
    public class ChatHub : Hub
    {
        public async Task SendMessage(string senderName, string receiverId, string message)
        {
            await Clients.User(receiverId).SendAsync("ReceiveMessage", new
            {
                senderId = Context.UserIdentifier,
                senderName = senderName,
                message = message,
                timestamp = DateTime.UtcNow
            });

            await Clients.Caller.SendAsync("ReceiveMessage", new
            {
                senderId = Context.UserIdentifier,
                senderName = senderName,
                message = message,
                timestamp = DateTime.UtcNow
            });
        }

        public async Task JoinConversation(string otherUserId)
        {
            var groupName = GetGroupName(Context.UserIdentifier!, otherUserId);
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        public async Task SendGroupMessage(string otherUserId, string senderName, string message)
        {
            var groupName = GetGroupName(Context.UserIdentifier!, otherUserId);
            await Clients.Group(groupName).SendAsync("ReceiveMessage", new
            {
                senderId = Context.UserIdentifier,
                senderName = senderName,
                message = message,
                timestamp = DateTime.UtcNow
            });
        }

        // Notifiera alla om nytt uppdrag
        public async Task NotifyNewMission(string title, int missionId)
        {
            await Clients.All.SendAsync("NewMission", new
            {
                title = title,
                missionId = missionId,
                timestamp = DateTime.UtcNow
            });
        }

        private static string GetGroupName(string userId1, string userId2)
        {
            var ids = new[] { userId1, userId2 }.OrderBy(x => x).ToArray();
            return $"chat_{ids[0]}_{ids[1]}";
        }
    }
}
