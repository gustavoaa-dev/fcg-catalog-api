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
            dto.GameId = id;
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
}
