using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Template.Web.Hubs;

[Authorize]
public sealed class ImtsHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        if (Context.UserIdentifier is { Length: > 0 } userId)
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user:{userId}");
        foreach (var role in Context.User?.Claims.Where(x => x.Type.EndsWith("/role")).Select(x => x.Value) ?? [])
            await Groups.AddToGroupAsync(Context.ConnectionId, $"role:{role}");
        await base.OnConnectedAsync();
    }
}
