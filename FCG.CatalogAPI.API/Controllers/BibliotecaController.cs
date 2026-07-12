using FCG.CatalogAPI.Application.DTOs;
using FCG.CatalogAPI.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FCG.CatalogAPI.API.Controllers;

[ApiController]
[Route("api/biblioteca")]
public class BibliotecaController : ControllerBase
{
    private readonly GameService _gameService;

    public BibliotecaController(GameService gameService)
    {
        _gameService = gameService;
    }

    [HttpGet("{userId:guid}")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<UserGameResponseDTO>>> ObterBiblioteca(Guid userId)
    {
        try
        {
            var jogos = await _gameService.ObterBiblioteca(userId);
            return Ok(jogos);
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
    }
}
