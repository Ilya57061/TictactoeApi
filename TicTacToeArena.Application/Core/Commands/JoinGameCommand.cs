using MediatR;
using Microsoft.EntityFrameworkCore;
using TicTacToeArena.Application.Abstractions;
using TicTacToeArena.Application.Common.Errors;
using TicTacToeArena.Application.DTOs;
using TicTacToeArena.Domain.Entities;
using TicTacToeArena.Domain.Enums;

namespace TicTacToeArena.Application.Core.Commands;

public sealed record JoinGameCommand(Guid GameId, string PlayerName) : IRequest<GameDto>;

public sealed class JoinGameCommandHandler : IRequestHandler<JoinGameCommand, GameDto>
{
    private readonly IRepository<Player, Guid> _players;
    private readonly IRepository<GameSession, Guid> _games;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IGameNotifier _notifier;

    public JoinGameCommandHandler(
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

    public async Task<GameDto> Handle(JoinGameCommand request, CancellationToken ct)
    {
        var name = request.PlayerName.Trim();

        var player = await _players.FirstOrDefaultAsync(
            p => p.Name.ToLower() == name.ToLower(),
            ct: ct);

        if (player is null)
            throw new AppException("PLAYER_NOT_FOUND", "Unknown player. Please enter name again.", 404);

        var game = await _games.FirstOrDefaultAsync(
            g => g.Id == request.GameId,
            queryShaper: q => q
                .Include(x => x.PlayerX)
                .Include(x => x.PlayerO),
            asNoTracking: false,
            ct: ct);

        if (game is null)
            throw new AppException("GAME_NOT_FOUND", "Game not found.", 404);

        if (game.Status != GameStatus.Open)
            throw new AppException("GAME_NOT_OPEN", "This game is not open for joining.", 409);

        if (game.PlayerXId == player.Id)
            throw new AppException("CANT_JOIN_OWN_GAME", "You created this game. Waiting for opponent.", 409);

        if (game.PlayerOId is not null)
            throw new AppException("GAME_FULL", "Another player already joined.", 409);

        game.PlayerOId = player.Id;
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

        await _notifier.LobbyUpdatedAsync(ct);
        await _notifier.GameUpdatedAsync(dto, ct);

        return dto;
    }
}