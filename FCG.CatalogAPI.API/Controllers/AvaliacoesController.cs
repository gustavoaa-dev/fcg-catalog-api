using FCG.CatalogAPI.Application.DTOs;
using FCG.CatalogAPI.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FCG.CatalogAPI.API.Controllers;

[ApiController]
[Route("api/jogos/{gameId:guid}/avaliacoes")]
public class AvaliacoesController : ControllerBase
{
    private readonly ReviewService _reviewService;

    public AvaliacoesController(ReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    // As duas formas submetem a mesma avaliação com a mesma semântica de upsert por
    // (gameId, userId) e os mesmos 201/200: a §117 da spec escreve POST, e o PUT é a escolha
    // idempotente do D9 — aceitar os dois evita 405 em quem segue o enunciado ao pé da letra.
    [HttpPut]
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ReviewResponseDTO>> Avaliar(Guid gameId, [FromBody] CriarAvaliacaoDTO dto)
    {
        var userId = ObterUsuarioId();
        var (avaliacao, criada) = await _reviewService.AvaliarAsync(gameId, userId, dto);

        return criada ? Created(string.Empty, avaliacao) : Ok(avaliacao);
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<ReviewResponseDTO>>> ObterPorJogo(Guid gameId)
    {
        return Ok(await _reviewService.ObterPorJogoAsync(gameId));
    }

    [HttpGet("resumo")]
    [Authorize]
    public async Task<ActionResult<ReviewResumoDTO>> ObterResumo(Guid gameId)
    {
        return Ok(await _reviewService.ObterResumoAsync(gameId));
    }

    /// <summary>
    /// O autor da avaliação é o usuário do token (claim "Id", o mesmo usado pela
    /// users-api e pela biblioteca do monolito) — nunca um campo do corpo.
    /// </summary>
    private Guid ObterUsuarioId()
    {
        var claimId = User.FindFirst("Id")?.Value;
        if (claimId is null || !Guid.TryParse(claimId, out var userId))
            throw new UnauthorizedAccessException("O token não contém um identificador de usuário válido.");

        return userId;
    }
}
