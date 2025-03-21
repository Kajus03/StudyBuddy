using Microsoft.AspNetCore.SignalR;
using StudyBuddy.Shared.Models;
using StudyBuddy.Shared.Utilities;
using StudyBuddy.Shared.ValueObjects;

namespace StudyBuddy.Hubs;

public class ChatHub : Hub
{
    private readonly IHttpClientFactory _clientFactory;

    public ChatHub(IHttpClientFactory clientFactory) => _clientFactory = clientFactory;
    private static HashSet<Guid> activeUsers = new HashSet<Guid>();

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        HttpContext? httpContext = Context.GetHttpContext();

        if (httpContext is null)
        {
            throw new NullReferenceException("HttpContext is null");
        }

        string sender = httpContext.Request.Query["sender"]!;
        string receiver = httpContext.Request.Query["receiver"]!;

        if (!string.IsNullOrEmpty(sender) && Guid.TryParse(sender, out Guid senderGuid))
        {
            // Mark the user as inactive and notify other users
            activeUsers.Remove(senderGuid);

            if (Guid.TryParse(receiver, out Guid receiverGuid))
            {
                Guid groupName = ConversationIdHelper.GetGroupId(senderGuid, receiverGuid);
                await Clients.Group(groupName.ToString()).SendAsync("UserInactive", senderGuid);
            }
        }

        await base.OnDisconnectedAsync(exception);
    }
    public override async Task OnConnectedAsync()
    {
        HttpContext? httpContext = Context.GetHttpContext();

        if (httpContext is null)
        {
            throw new NullReferenceException("HttpContext is null");
        }

        string receiver = httpContext.Request.Query["receiver"]!;
        string sender = httpContext.Request.Query["sender"]!;

        if (!string.IsNullOrEmpty(sender) && !string.IsNullOrEmpty(receiver))
        {
            if (!Guid.TryParse(receiver, out Guid receiverGuid) || !Guid.TryParse(sender, out Guid senderGuid))
            {
                throw new ArgumentException("Invalid Guid");
            }

            Guid groupName = ConversationIdHelper.GetGroupId(senderGuid, receiverGuid);

            // Add the current connection to the conversation group
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName.ToString());

            // Send a "UserActive" message to the group
            activeUsers.Add(senderGuid);

            // Send a "UserActive" message for each active user
            foreach (Guid activeUser in activeUsers)
            {
                await Clients.Group(groupName.ToString()).SendAsync("UserActive", activeUser);
            }
        }

        await base.OnConnectedAsync();
    }

    public async Task SendMessageToGroup(Guid groupName, string message)
    {
        HttpContext? httpContext = Context.GetHttpContext();

        if (httpContext is null)
        {
            throw new NullReferenceException("HttpContext is null");
        }

        string sender = httpContext.Request.Query["sender"]!;

        if (!UserId.TryParse(sender, out UserId senderId))
        {
            throw new ArgumentException("Invalid Guid");
        }

        ChatMessage chatMessage = new(message, DateTime.UtcNow, senderId, groupName);

        HttpClient httpClient = _clientFactory.CreateClient("StudyBuddy.API");
        var responseAddMessage =
            await httpClient.PostAsJsonAsync("api/v1/chat/messages/add", chatMessage);

        responseAddMessage.EnsureSuccessStatusCode();

        // Broadcast the message to the conversation group
        await Clients.Group(groupName.ToString()).SendAsync("ReceiveMessage", sender, message);
    }


    public async Task ReceiveMessage(string user, string message) =>
        await Clients.All.SendAsync("ReceiveMessage", user, message);
}
