using TicTacToeArena.Domain.Enums;

namespace TicTacToeArena.Application.DTOs;

public sealed record GameDto(
    Guid Id,
    GameStatus Status,
    Mark[] Board,
    Mark Winner,
    string PlayerXName,
    string? PlayerOName,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc
);
