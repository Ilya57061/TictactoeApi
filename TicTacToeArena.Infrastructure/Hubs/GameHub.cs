using MediatR;
using Microsoft.AspNetCore.SignalR;

namespace TicTacToeArena.Infrastructure.Hubs;

public sealed class GameHub : Hub
{
    public async Task JoinLobby()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "lobby");
    }

    public async Task LeaveLobby()
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "lobby");
    }

    public async Task JoinGame(string gameId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"game:{gameId}");

        if (ConnectionMap.Connections.TryGetValue(Context.ConnectionId, out var info))
            ConnectionMap.Connections[Context.ConnectionId] = (Guid.Parse(gameId), info.PlayerName);
    }

    public async Task LeaveGame(string gameId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"game:{gameId}");

        if (ConnectionMap.Connections.TryGetValue(Context.ConnectionId, out var info))
            ConnectionMap.Connections[Context.ConnectionId] = (null, info.PlayerName);
    }

    public override async Task OnConnectedAsync()
    {
        var playerName = Context.GetHttpContext()?.Request.Query["player"];
        if (!string.IsNullOrWhiteSpace(playerName))
        {
            ConnectionMap.Connections[Context.ConnectionId] = (null, playerName!);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (ConnectionMap.Connections.TryRemove(Context.ConnectionId, out var info))
        {
            if (info.GameId.HasValue)
            {
                await Clients.Group($"game:{info.GameId.Value}")
                    .SendAsync("PlayerLeft", info.PlayerName);

                await Clients.Group("lobby")
                    .SendAsync("LobbyUpdated");
            }
        }

        await base.OnDisconnectedAsync(exception);
    }
}