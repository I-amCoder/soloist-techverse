using Microsoft.AspNetCore.SignalR;

public class ChatHub : Hub
{
    public async Task SendMessage(string userId, string message)
    {
        Console.WriteLine($"Sending message to {userId}: {message}");
        await Clients.User(userId).SendAsync("ReceiveMessage", message);
    }
} 