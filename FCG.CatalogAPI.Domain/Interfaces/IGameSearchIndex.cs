using FCG.CatalogAPI.Domain.Entities;

namespace FCG.CatalogAPI.Domain.Interfaces;

/// <summary>
/// Índice de busca do catálogo. Existe separado do SQL porque a busca textual
/// (fuzzy, relevância) não é o que o repositório relacional faz bem.
/// </summary>
public interface IGameSearchIndex
{
    Task IndexarAsync(Game game);
    Task RemoverAsync(Guid id);
    Task<IReadOnlyList<Game>> BuscarAsync(string termo);
    Task ReindexarAsync(IEnumerable<Game> games);
}
