using MediatR;
using Microsoft.EntityFrameworkCore;
using TicTacToeArena.Application.Abstractions;
using TicTacToeArena.Application.DTOs;
using TicTacToeArena.Domain.Entities;
using TicTacToeArena.Domain.Enums;

namespace TicTacToeArena.Application.Core.Queries;

public sealed record GetOpenGamesQuery : IRequest<IReadOnlyList<GameDto>>;

public sealed class GetOpenGamesQueryHandler : IRequestHandler<GetOpenGamesQuery, IReadOnlyList<GameDto>>
{
    private readonly IRepository<GameSession, Guid> _games;

    public GetOpenGamesQueryHandler(IRepository<GameSession, Guid> games)
    {
        _games = games;
    }

    public async Task<IReadOnlyList<GameDto>> Handle(GetOpenGamesQuery request, CancellationToken ct)
    {
        var games = await _games.ListAsync(
            predicate: g => g.Status == GameStatus.Open,
            queryShaper: q => q
                .OrderByDescending(x => x.CreatedAtUtc)
                .Include(x => x.PlayerX)
                .Include(x => x.PlayerO)
                .Take(50),
            asNoTracking: true,
            ct: ct);

        return games.Select(g => g.ToDto()).ToList();
    }
}