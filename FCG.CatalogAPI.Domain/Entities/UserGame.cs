namespace FCG.CatalogAPI.Domain.Entities;

public class UserGame
{
    public Guid UserId { get; private set; }
    public Guid GameId { get; private set; }
    public DateTime DataCompra { get; private set; }
    public Game Game { get; private set; } = null!;

    private UserGame()
    {
    }

    public UserGame(Guid userId, Guid gameId)
    {
        UserId = userId;
        GameId = gameId;
        DataCompra = DateTime.UtcNow;
    }
}
