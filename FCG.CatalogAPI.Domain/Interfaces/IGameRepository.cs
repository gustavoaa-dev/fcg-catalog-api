using FCG.CatalogAPI.Domain.Entities;

namespace FCG.CatalogAPI.Domain.Interfaces;

public interface IGameRepository
{
    Task<Game?> ObterPorId(Guid id);
    Task<IEnumerable<Game>> ObterTodos();
    Task Adicionar(Game game);
    Task Remover(Game game);
    Task Salvar();
}
