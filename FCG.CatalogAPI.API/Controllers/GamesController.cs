using FCG.CatalogAPI.Application.DTOs;
using FCG.CatalogAPI.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FCG.CatalogAPI.API.Controllers;

[ApiController]
[Route("api/jogos")]
public class GamesController : ControllerBase
{
    private readonly GameService _gameService;

    public GamesController(GameService gameService)
    {
        _gameService = gameService;
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<GameResponseDTO>>> ObterTodos()
    {
        try
        {
            var games = await _gameService.ObterTodos();
            return Ok(games);
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
    }

    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<ActionResult<GameResponseDTO>> ObterPorId(Guid id)
    {
        try
        {
            var game = await _gameService.ObterPorId(id);
            return Ok(game);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { mensagem = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<GameResponseDTO>> Criar([FromBody] CriarGameDTO dto)
    {
        try
        {
            var game = await _gameService.CriarGame(dto);
            return Created(string.Empty, game);
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Remover(Guid id)
    {
        try
        {
            await _gameService.Remover(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { mensagem = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
    }

    [HttpPost("{id:guid}/comprar")]
    [Authorize]
    public async Task<IActionResult> Comprar(Guid id, [FromBody] IniciarCompraDTO dto)
    {
        try
        {
            // O autor da compra é o usuário do token, nunca o do corpo.
            dto.GameId = id;
            dto.UserId = ObterUsuarioId();
            var mensagem = await _gameService.IniciarCompra(dto);
            return Accepted(new { mensagem });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { mensagem = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] CriarGameDTO dto)
    {
        try
        {
            await _gameService.Atualizar(id, dto);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { mensagem = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
    }

    /// <summary>
    /// Busca textual no índice, com tolerância a erro de digitação e ordem por relevância.
    /// Não consulta o SQL: é o Elasticsearch que responde.
    /// </summary>
    [HttpGet("search")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<GameResponseDTO>>> Buscar([FromQuery] string q)
    {
        if (string.IsNullOrWhiteSpace(q))
            return BadRequest(new { mensagem = "Informe o termo de busca(ex.: jogo)." });

        return Ok(await _gameService.Buscar(q));
    }

    /// <summary>O mesmo claim "Id" que a users-api assina e a biblioteca do monolito lê.</summary>
    private Guid ObterUsuarioId()
    {
        var claimId = User.FindFirst("Id")?.Value;
        if (claimId is null || !Guid.TryParse(claimId, out var userId))
            throw new UnauthorizedAccessException("O token não contém um identificador de usuário válido.");

        return userId;
    }
}
