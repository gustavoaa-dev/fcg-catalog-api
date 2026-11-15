using System.Security.Claims;
using FCG.CatalogAPI.Application.DTOs;
using FCG.CatalogAPI.Application.Exceptions;
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
        VerificarAcesso(userId);
        return Ok(await _gameService.ObterBiblioteca(userId));
    }

    // Admin lê a biblioteca de qualquer um; usuário comum, só a própria.
    private void VerificarAcesso(Guid userId)
    {
        if (User.IsInRole("Admin"))
            return;

        var claimId = User.FindFirst("Id")?.Value;
        if (claimId is null || !Guid.TryParse(claimId, out var tokenUserId) || tokenUserId != userId)
            throw new AcessoNegadoException("Você não tem permissão para acessar a biblioteca de outro usuário.");
    }
}
