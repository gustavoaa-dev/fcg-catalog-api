using FCG.CatalogAPI.Domain.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace FCG.CatalogAPI.Application.Consumers;

public class PaymentProcessedConsumer : IConsumer<PaymentProcessedEvent>
{
    private readonly Services.GameService _gameService;
    private readonly ILogger<PaymentProcessedConsumer> _logger;

    public PaymentProcessedConsumer(Services.GameService gameService, ILogger<PaymentProcessedConsumer> logger)
    {
        _gameService = gameService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PaymentProcessedEvent> context)
    {
        var evento = context.Message;

        if (evento.Status == "Approved")
        {
            await _gameService.AdicionarJogoAoBiblioteca(evento.UserId, evento.GameId);
            _logger.LogInformation(
                "Pagamento aprovado - Jogo {GameId} adicionado à biblioteca do usuário {UserId} (Order: {OrderId})",
                evento.GameId, evento.UserId, evento.OrderId);
        }
        else
        {
            _logger.LogWarning(
                "Pagamento rejeitado - Jogo {GameId} NÃO adicionado para o usuário {UserId} (Order: {OrderId})",
                evento.GameId, evento.UserId, evento.OrderId);
        }
    }
}
