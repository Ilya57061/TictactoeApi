using TicTacToeArena.Domain.Entities;
using TicTacToeArena.Domain.Enums;

namespace TicTacToeArena.Application.DTOs;

public static class MappingExtensions
{
    public static PlayerDto ToDto(this Player p)
    {
       return new PlayerDto(p.Id, p.Name, p.Played, p.Wins, p.Losses, p.Draws);
    }

    public static GameDto ToDto(this GameSession g)
    {
        return new GameDto (
            g.Id,
            g.Status, g.Board,
            g.Winner,
            g.PlayerX?.Name ?? "?",
            g.PlayerO?.Name,
            g.CreatedAtUtc,
            g.UpdatedAtUtc
            );
    }
}
