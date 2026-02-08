using MediatR;
using TicTacToeArena.Application.Abstractions;
using TicTacToeArena.Application.Common.Errors;
using TicTacToeArena.Application.DTOs;
using TicTacToeArena.Domain.Entities;

namespace TicTacToeArena.Application.Core.Queries;

public sealed record GetPlayerStatsQuery(string Name) : IRequest<PlayerDto>;

public sealed class GetPlayerStatsQueryHandler : IRequestHandler<GetPlayerStatsQuery, PlayerDto>
{
    private readonly IRepository<Player, Guid> _players;

    public GetPlayerStatsQueryHandler(IRepository<Player, Guid> players)
    {
        _players = players;
    }

    public async Task<PlayerDto> Handle(GetPlayerStatsQuery request, CancellationToken ct)
    {
        var name = request.Name.Trim();

        var player = await _players.FirstOrDefaultAsync(
            p => p.Name.ToLower() == name.ToLower(),
            asNoTracking: true,
            ct: ct);

        if (player is null)
            throw new AppException("PLAYER_NOT_FOUND", "Player not found.", 404);

        return player.ToDto();
    }
}