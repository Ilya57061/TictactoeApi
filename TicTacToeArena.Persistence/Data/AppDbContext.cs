using Microsoft.EntityFrameworkCore;
using TicTacToeArena.Domain.Entities;

namespace TicTacToeArena.Persistence.Data;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Player>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Name).IsRequired().HasMaxLength(64);
            b.HasIndex(x => x.Name).IsUnique();
        });

        modelBuilder.Entity<GameSession>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Board).IsRequired().HasMaxLength(9);

            b.HasOne(x => x.PlayerX)
                .WithMany()
                .HasForeignKey(x => x.PlayerXId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(x => x.PlayerO)
                .WithMany()
                .HasForeignKey(x => x.PlayerOId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        base.OnModelCreating(modelBuilder);
    }

    public DbSet<Player> Players => Set<Player>();
    public DbSet<GameSession> Games => Set<GameSession>();
}
