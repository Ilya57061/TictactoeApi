using TicTacToeArena.Domain.Common;

namespace TicTacToeArena.Domain.Entities;

public class Player : IEntity<Guid>
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = default!;
    public int Played { get; set; }
    public int Wins { get; set; }
    public int Losses { get; set; }
    public int Draws { get; set; }

    private Player() { }
    public Player(string name) => Name = name;
}