using MediatR;
using Microsoft.EntityFrameworkCore;
using TicTacToeArena.Application.Abstractions;
using TicTacToeArena.Application.Common.Errors;
using TicTacToeArena.Application.DTOs;
using TicTacToeArena.Domain.Entities;

namespace TicTacToeArena.Application.Core.Queries;

public sealed record GetGameQuery(Guid GameId) : IRequest<GameDto>;

public sealed class GetGameQueryHandler : IRequestHandler<GetGameQuery, GameDto>
{
    private readonly IRepository<GameSession, Guid> _games;

    public GetGameQueryHandler(IRepository<GameSession, Guid> games)
    {
        _games = games;
    }

    public async Task<GameDto> Handle(GetGameQuery request, CancellationToken ct)
    {
        var game = await _games.FirstOrDefaultAsync(
            g => g.Id == request.GameId,
            queryShaper: q => q
                .Include(x => x.PlayerX)
                .Include(x => x.PlayerO),
            asNoTracking: true,
            ct: ct);

        if (game is null)
            throw new AppException("GAME_NOT_FOUND", "Game not found.", 404);

        return game.ToDto();
    }
}