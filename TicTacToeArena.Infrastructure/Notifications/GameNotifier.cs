using Microsoft.AspNetCore.SignalR;
using TicTacToeArena.Application.Abstractions;
using TicTacToeArena.Application.DTOs;
using TicTacToeArena.Infrastructure.Hubs;

namespace TicTacToeArena.Infrastructure.Notifications;

public sealed class GameNotifier : IGameNotifier
{
    private readonly IHubContext<GameHub> _hub;

    public GameNotifier(IHubContext<GameHub> hub)
    {
        _hub = hub;
    }

    public Task GameUpdatedAsync(GameDto game, CancellationToken ct)
    {
        return _hub.Clients.Group($"game:{game.Id}").SendAsync("GameUpdated", game, ct);
    }

    public Task LobbyUpdatedAsync(CancellationToken ct)
    {
        return _hub.Clients.Group("lobby").SendAsync("LobbyUpdated", ct);
    }
}
