using Microsoft.AspNetCore.SignalR;
namespace AspnetCoreMvcFull.Hubs;

public class ItemHub : Hub
{
    public async Task NotifyNewItem(object itemData)
    {
        await Clients.All.SendAsync("ReceiveNewItemNotification", itemData);
    }
    
    public async Task JoinGroup(string groupName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
    }
    
    public async Task LeaveGroup(string groupName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
    }
}
