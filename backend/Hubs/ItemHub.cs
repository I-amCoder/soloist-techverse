using Microsoft.AspNetCore.SignalR;
namespace AspnetCoreMvcFull.Hubs;

public class ItemHub : Hub
{
    public async Task NotifyNewItem(string itemType)
    {
        await Clients.All.SendAsync("ReceiveNewItemNotification", itemType);
    }
}
