using TicTacToeArena.Application.DTOs;

namespace TicTacToeArena.Application.Abstractions;

public interface IGameNotifier
{
    Task GameUpdatedAsync(GameDto game, CancellationToken ct);
    Task LobbyUpdatedAsync(CancellationToken ct);
}
