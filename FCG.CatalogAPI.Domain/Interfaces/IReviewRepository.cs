using FCG.CatalogAPI.Domain.Entities;

namespace FCG.CatalogAPI.Domain.Interfaces;

public interface IReviewRepository
{
    /// <summary>
    /// Insere ou substitui a avaliação do par (jogo, usuário). Devolve a avaliação
    /// <b>como está persistida</b> (em uma atualização o id e a dataCriacao são os originais)
    /// e true quando criou.
    /// </summary>
    Task<(Review Avaliacao, bool Criada)> UpsertAsync(Review review);

    Task<IEnumerable<Review>> ObterPorJogoAsync(Guid gameId);

    Task<ReviewResumo> ObterResumoAsync(Guid gameId);

    /// <summary>Cria o índice único de (jogo, usuário). Chamado uma vez no boot da API.</summary>
    Task InicializarAsync();
}
