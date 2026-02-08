using MediatR;
using Microsoft.EntityFrameworkCore;
using TicTacToeArena.Application.Abstractions;
using TicTacToeArena.Application.Common.Errors;
using TicTacToeArena.Application.DTOs;
using TicTacToeArena.Domain.Entities;
using TicTacToeArena.Domain.Enums;

namespace TicTacToeArena.Application.Core.Commands;

public sealed record RematchGameCommand(Guid GameId, string PlayerName) : IRequest<GameDto>;

public sealed class RematchGameCommandHandler : IRequestHandler<RematchGameCommand, GameDto>
{
    private readonly IRepository<Player, Guid> _players;
    private readonly IRepository<GameSession, Guid> _games;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IGameNotifier _notifier;

    public RematchGameCommandHandler(
        IRepository<Player, Guid> players,
        IRepository<GameSession, Guid> games,
        IUnitOfWork unitOfWork,
        IGameNotifier notifier)
    {
        _players = players;
        _games = games;
        _unitOfWork = unitOfWork;
        _notifier = notifier;
    }

    public async Task<GameDto> Handle(RematchGameCommand request, CancellationToken ct)
    {
        var playerName = request.PlayerName.Trim();

        var player = await _players.FirstOrDefaultAsync(
            p => p.Name.ToLower() == playerName.ToLower(),
            ct: ct);

        if (player is null)
            throw new AppException("PLAYER_NOT_FOUND", "Player not found.", 404);

        var game = await _games.FirstOrDefaultAsync(
            g => g.Id == request.GameId,
            queryShaper: q => q
                .Include(x => x.PlayerX)
                .Include(x => x.PlayerO),
            asNoTracking: false,
            ct: ct);

        if (game is null)
            throw new AppException("GAME_NOT_FOUND", "Game not found.", 404);

        if (game.PlayerXId != player.Id && game.PlayerOId != player.Id)
            throw new AppException("INVALID_PLAYER", "Player is not part of the game.", 400);

        if (game.PlayerXId is null || game.PlayerOId is null)
            throw new AppException("NEED_TWO_PLAYERS", "Both players must be present to start a new round.", 409);

        if (game.Status != GameStatus.Finished)
            throw new AppException("GAME_NOT_FINISHED", "You can start a new round only after the current one is finished.", 409);

        game.Board = new Mark[9];
        game.Winner = Mark.Empty;
        game.Status = GameStatus.InProgress;
        game.UpdatedAtUtc = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(ct);

        var gameForDto = await _games.FirstOrDefaultAsync(
            g => g.Id == request.GameId,
            queryShaper: q => q
                .Include(x => x.PlayerX)
                .Include(x => x.PlayerO),
            asNoTracking: true,
            ct: ct);

        var dto = gameForDto!.ToDto();

        await _notifier.GameUpdatedAsync(dto, ct);
        await _notifier.LobbyUpdatedAsync(ct);

        return dto;
    }
}