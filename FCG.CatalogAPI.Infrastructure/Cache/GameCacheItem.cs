using FCG.CatalogAPI.Domain.Entities;

namespace FCG.CatalogAPI.Infrastructure.Cache;

/// <summary>
/// Payload serializado no Redis. É um tipo próprio (e não a entidade Game) porque
/// a entidade tem setters privados e não pode ser desserializada por System.Text.Json.
/// </summary>
public class GameCacheItem
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public decimal Preco { get; set; }
    public DateTime DataCadastro { get; set; }

    public static GameCacheItem De(Game game) => new()
    {
        Id = game.Id,
        Nome = game.Nome,
        Descricao = game.Descricao,
        Preco = game.Preco,
        DataCadastro = game.DataCadastro
    };

    public Game ParaEntidade() => new(Id, Nome, Descricao, Preco, DataCadastro);
}
