
using TicTacToeArena.Domain.Common;
using TicTacToeArena.Domain.Enums;

namespace TicTacToeArena.Domain.Entities;

public sealed class GameSession : IEntity<Guid>
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string HostName { get; set; } = default!;
    public string? OpponentName { get; set; }
    public GameStatus Status { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public Guid? PlayerXId { get; set; }
    public Player? PlayerX { get; set; }

    public Guid? PlayerOId { get; set; }
    public Player? PlayerO { get; set; }

    public Mark[] Board { get; set; } = new Mark[9];
    public Mark Winner { get; set; } = Mark.Empty;

    public GameSession(string hostName)
    {
        HostName = hostName;
    }

}
