using FCG.CatalogAPI.Application.DTOs;
using FCG.CatalogAPI.Domain.Entities;
using FCG.CatalogAPI.Domain.Interfaces;

namespace FCG.CatalogAPI.Application.Services;

public class ReviewService
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IGameRepository _gameRepository;

    public ReviewService(IReviewRepository reviewRepository, IGameRepository gameRepository)
    {
        _reviewRepository = reviewRepository;
        _gameRepository = gameRepository;
    }

    public async Task<(ReviewResponseDTO Avaliacao, bool Criada)> AvaliarAsync(Guid gameId, Guid userId, CriarAvaliacaoDTO dto)
    {
        if (dto is null)
            throw new ArgumentException("Os dados da avaliação são obrigatórios.");

        if (dto.Nota < 1 || dto.Nota > 5)
            throw new ArgumentException("A nota deve estar entre 1 e 5.");

        // O jogo precisa existir no SQL: avaliação órfã não faz sentido.
        var jogo = await _gameRepository.ObterPorId(gameId);
        if (jogo is null)
            throw new KeyNotFoundException("Jogo não encontrado.");

        var agora = DateTime.UtcNow;
        var review = new Review
        {
            Id = Guid.NewGuid(),
            GameId = gameId,
            UserId = userId,
            Nota = dto.Nota,
            Comentario = string.IsNullOrWhiteSpace(dto.Comentario) ? null : dto.Comentario.Trim(),
            Tags = (dto.Tags ?? new List<string>())
                .Select(t => t?.Trim() ?? string.Empty)
                .Where(t => t.Length > 0)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList(),
            DataCriacao = agora,
            DataAtualizacao = agora
        };

        // O Id e a DataCriacao acima so valem para o SetOnInsert: em uma atualizacao o
        // documento mantem o _id e a dataCriacao originais, entao a resposta tem que vir
        // do documento persistido — senao o corpo do PUT contradiz o do GET.
        var (persistida, criada) = await _reviewRepository.UpsertAsync(review);
        return (Mapear(persistida), criada);
    }

    public async Task<IEnumerable<ReviewResponseDTO>> ObterPorJogoAsync(Guid gameId)
    {
        var jogo = await _gameRepository.ObterPorId(gameId);
        if (jogo is null)
            throw new KeyNotFoundException("Jogo não encontrado.");

        var reviews = await _reviewRepository.ObterPorJogoAsync(gameId);
        return reviews.Select(Mapear).ToList();
    }

    public async Task<ReviewResumoDTO> ObterResumoAsync(Guid gameId)
    {
        var jogo = await _gameRepository.ObterPorId(gameId);
        if (jogo is null)
            throw new KeyNotFoundException("Jogo não encontrado.");

        var resumo = await _reviewRepository.ObterResumoAsync(gameId);
        return new ReviewResumoDTO
        {
            JogoId = gameId,
            Total = resumo.Total,
            NotaMedia = resumo.NotaMedia
        };
    }

    private static ReviewResponseDTO Mapear(Review review) => new()
    {
        Id = review.Id,
        GameId = review.GameId,
        UsuarioId = review.UserId,
        Nota = review.Nota,
        Comentario = review.Comentario,
        Tags = review.Tags,
        DataCriacao = review.DataCriacao,
        DataAtualizacao = review.DataAtualizacao
    };
}
