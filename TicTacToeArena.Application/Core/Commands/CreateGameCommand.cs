using MediatR;
using Microsoft.EntityFrameworkCore;
using TicTacToeArena.Application.Abstractions;
using TicTacToeArena.Application.Common.Errors;
using TicTacToeArena.Application.DTOs;
using TicTacToeArena.Domain.Entities;
using TicTacToeArena.Domain.Enums;

namespace TicTacToeArena.Application.Core.Commands;

public sealed record CreateGameCommand(string PlayerName) : IRequest<GameDto>;

public sealed class CreateGameCommandHandler : IRequestHandler<CreateGameCommand, GameDto>
{
    private readonly IRepository<Player, Guid> _players;
    private readonly IRepository<GameSession, Guid> _games;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IGameNotifier _notifier;

    public CreateGameCommandHandler(
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

    public async Task<GameDto> Handle(CreateGameCommand request, CancellationToken ct)
    {
        var hostName = request.PlayerName.Trim();

        if (string.IsNullOrWhiteSpace(hostName))
            throw new AppException("INVALID_NAME", "Name is required.", 400);

        var host = await _players.FirstOrDefaultAsync(
            p => p.Name.ToLower() == hostName.ToLower(),
            ct: ct);

        if (host is null)
            throw new AppException("PLAYER_NOT_FOUND", "Unknown player. Please login first.", 404);

        var game = new GameSession(hostName)
        {
            Status = GameStatus.Open,
            PlayerXId = host.Id,
            UpdatedAtUtc = DateTime.UtcNow
        };

        await _games.AddAsync(game, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        var gameForDto = await _games.FirstOrDefaultAsync(
            g => g.Id == game.Id,
            queryShaper: q => q
                .Include(x => x.PlayerX)
                .Include(x => x.PlayerO),
            asNoTracking: true,
            ct: ct);

        var dto = gameForDto!.ToDto();

        await _notifier.LobbyUpdatedAsync(ct);

        return dto;
    }
}