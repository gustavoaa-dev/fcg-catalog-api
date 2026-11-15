using System.Security.Claims;
using FCG.CatalogAPI.API.Controllers;
using FCG.CatalogAPI.Application.DTOs;
using FCG.CatalogAPI.Application.Services;
using FCG.CatalogAPI.Domain.Entities;
using FCG.CatalogAPI.Domain.Interfaces;
using FCG.Shared.Events;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace FCG.CatalogAPI.Tests;

public class AutorizacaoTests
{
    [Fact]
    public async Task Comprar_DeveIgnorarUserIdDoCorpo()
    {
        var userDoToken = Guid.NewGuid();
        OrderPlacedEvent? publicado = null;

        var games = new Mock<IGameRepository>();
        games.Setup(g => g.ObterPorId(It.IsAny<Guid>())).ReturnsAsync(new Game("Jogo", "Desc", 10m));
        var userGames = new Mock<IUserGameRepository>();
        userGames.Setup(u => u.ObterPorIds(It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync((UserGame?)null);
        var publish = new Mock<IPublishEndpoint>();
        publish.Setup(p => p.Publish(It.IsAny<OrderPlacedEvent>(), It.IsAny<CancellationToken>()))
            .Callback<OrderPlacedEvent, CancellationToken>((evento, _) => publicado = evento)
            .Returns(Task.CompletedTask);

        var service = new GameService(games.Object, userGames.Object, publish.Object,
            Mock.Of<IGameSearchIndex>(), NullLogger<GameService>.Instance);

        var controller = new GamesController(service)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(
                        new[] { new Claim("Id", userDoToken.ToString()) }, "test"))
                }
            }
        };

        await controller.Comprar(Guid.NewGuid(), new IniciarCompraDTO { UserId = Guid.NewGuid() });

        // O corpo mandou outro Guid; o evento tem que sair com o do token.
        Assert.Equal(userDoToken, publicado!.UserId);
    }
}
