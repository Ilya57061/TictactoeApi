using MediatR;
using Microsoft.EntityFrameworkCore;
using TicTacToeArena.Application.Abstractions;
using TicTacToeArena.Application.Common.Errors;
using TicTacToeArena.Application.DTOs;
using TicTacToeArena.Domain.Entities;
using TicTacToeArena.Domain.Enums;

namespace TicTacToeArena.Application.Core.Commands;

public sealed record MakeMoveCommand(Guid GameId, string PlayerName, int CellIndex) : IRequest<GameDto>;

public sealed class MakeMoveCommandHandler : IRequestHandler<MakeMoveCommand, GameDto>
{
    private readonly IRepository<Player, Guid> _players;
    private readonly IRepository<GameSession, Guid> _games;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IGameNotifier _notifier;

    public MakeMoveCommandHandler(
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

    public async Task<GameDto> Handle(MakeMoveCommand request, CancellationToken ct)
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

        if (game.Status != GameStatus.InProgress)
            throw new AppException("GAME_NOT_IN_PROGRESS", "Game is not in progress.", 409);

        if (request.CellIndex < 0 || request.CellIndex > 8)
            throw new AppException("INVALID_CELL", "Cell index must be between 0 and 8.", 400);

        var xCount = game.Board.Count(c => c == Mark.X);
        var oCount = game.Board.Count(c => c == Mark.O);
        var expectedTurn = (xCount == oCount) ? Mark.X : Mark.O;

        if (game.PlayerXId == player.Id)
        {
            if (expectedTurn != Mark.X)
                throw new AppException("NOT_YOUR_TURN", "It's not your turn.", 409);

            if (game.Board[request.CellIndex] != Mark.Empty)
                throw new AppException("INVALID_MOVE", "Cell is already occupied.", 400);

            game.Board[request.CellIndex] = Mark.X;
        }
        else if (game.PlayerOId == player.Id)
        {
            if (expectedTurn != Mark.O)
                throw new AppException("NOT_YOUR_TURN", "It's not your turn.", 409);

            if (game.Board[request.CellIndex] != Mark.Empty)
                throw new AppException("INVALID_MOVE", "Cell is already occupied.", 400);

            game.Board[request.CellIndex] = Mark.O;
        }
        else
        {
            throw new AppException("INVALID_PLAYER", "Player is not part of the game.", 400);
        }

        if (CheckWinner(game.Board, Mark.X))
        {
            game.Status = GameStatus.Finished;
            game.Winner = Mark.X;
        }
        else if (CheckWinner(game.Board, Mark.O))
        {
            game.Status = GameStatus.Finished;
            game.Winner = Mark.O;
        }
        else if (game.Board.All(cell => cell != Mark.Empty))
        {
            game.Status = GameStatus.Finished;
            game.Winner = Mark.Empty;
        }

        if (game.Status == GameStatus.Finished)
        {
            if (game.PlayerX is not null) game.PlayerX.Played++;
            if (game.PlayerO is not null) game.PlayerO.Played++;

            if (game.Winner == Mark.X)
            {
                if (game.PlayerX is not null) game.PlayerX.Wins++;
                if (game.PlayerO is not null) game.PlayerO.Losses++;
            }
            else if (game.Winner == Mark.O)
            {
                if (game.PlayerO is not null) game.PlayerO.Wins++;
                if (game.PlayerX is not null) game.PlayerX.Losses++;
            }
            else
            {
                if (game.PlayerX is not null) game.PlayerX.Draws++;
                if (game.PlayerO is not null) game.PlayerO.Draws++;
            }
        }

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

        if (dto.Status == GameStatus.Finished)
            await _notifier.LobbyUpdatedAsync(ct);

        return dto;
    }

    private static bool CheckWinner(Mark[] board, Mark mark)
    {
        int[,] winningCombinations = new int[,]
        {
            {0, 1, 2}, {3, 4, 5}, {6, 7, 8},
            {0, 3, 6}, {1, 4, 7}, {2, 5, 8},
            {0, 4, 8}, {2, 4, 6}
        };

        for (int i = 0; i < winningCombinations.GetLength(0); i++)
        {
            if (board[winningCombinations[i, 0]] == mark &&
                board[winningCombinations[i, 1]] == mark &&
                board[winningCombinations[i, 2]] == mark)
            {
                return true;
            }
        }

        return false;
    }
}