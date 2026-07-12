using FCG.CatalogAPI.Domain.Entities;

namespace FCG.CatalogAPI.Domain.Interfaces;

public interface IUserGameRepository
{
    Task<UserGame?> ObterPorIds(Guid userId, Guid gameId);
    Task<IEnumerable<UserGame>> ObterPorUsuario(Guid userId);
    Task Adicionar(UserGame userGame);
    Task Salvar();
}
