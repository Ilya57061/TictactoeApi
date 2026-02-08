namespace TicTacToeArena.Application.DTOs;

public sealed record PlayerDto(
    Guid Id,
    string Name,
    int Played,
    int Wins,
    int Losses,
    int Draws
);
