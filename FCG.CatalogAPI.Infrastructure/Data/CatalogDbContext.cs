using FCG.CatalogAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FCG.CatalogAPI.Infrastructure.Data;

public class CatalogDbContext : DbContext
{
    public DbSet<Game> Games { get; set; }
    public DbSet<UserGame> UserGames { get; set; }

    public CatalogDbContext(DbContextOptions<CatalogDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<UserGame>()
            .HasKey(ug => new { ug.UserId, ug.GameId });

        modelBuilder.Entity<Game>()
            .Property(g => g.Preco)
            .HasPrecision(10, 2);
    }
}
