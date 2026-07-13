using FCG.CatalogAPI.Application.DTOs;
using FCG.CatalogAPI.Domain.Entities;
using FCG.Shared.Events;
using FCG.CatalogAPI.Domain.Interfaces;
using MassTransit;

namespace FCG.CatalogAPI.Application.Services;

public class GameService
{
    private readonly IGameRepository _gameRepository;
    private readonly IUserGameRepository _userGameRepository;
    private readonly IPublishEndpoint _publishEndpoint;

    public GameService(
        IGameRepository gameRepository,
        IUserGameRepository userGameRepository,
        IPublishEndpoint publishEndpoint)
    {
        _gameRepository = gameRepository;
        _userGameRepository = userGameRepository;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<GameResponseDTO> CriarGame(CriarGameDTO dto)
    {
        if (dto is null)
            throw new ArgumentNullException(nameof(dto), "Os dados do jogo são obrigatórios.");

        var game = new Game(dto.Nome, dto.Descricao, dto.Preco);

        await _gameRepository.Adicionar(game);
        await _gameRepository.Salvar();

        return MapearParaGameResponse(game);
    }

    public async Task<IEnumerable<GameResponseDTO>> ObterTodos()
    {
        var games = await _gameRepository.ObterTodos();
        return games.Select(MapearParaGameResponse).ToList();
    }

    public async Task<GameResponseDTO> ObterPorId(Guid id)
    {
        var game = await _gameRepository.ObterPorId(id);
        if (game is null)
            throw new KeyNotFoundException("Jogo não encontrado.");

        return MapearParaGameResponse(game);
    }

    public async Task Remover(Guid id)
    {
        var game = await _gameRepository.ObterPorId(id);
        if (game is null)
            throw new KeyNotFoundException("Jogo não encontrado.");

        await _gameRepository.Remover(game);
        await _gameRepository.Salvar();
    }

    public async Task<string> IniciarCompra(IniciarCompraDTO dto)
    {
        var game = await _gameRepository.ObterPorId(dto.GameId);
        if (game is null)
            throw new KeyNotFoundException("Jogo não encontrado.");

        var userGameExistente = await _userGameRepository.ObterPorIds(dto.UserId, dto.GameId);
        if (userGameExistente is not null)
            throw new ArgumentException("O usuário já possui este jogo.");

        var orderPlacedEvent = new OrderPlacedEvent
        {
            OrderId = Guid.NewGuid(),
            UserId = dto.UserId,
            GameId = dto.GameId,
            Price = game.Preco,
            CreatedAt = DateTime.UtcNow
        };

        await _publishEndpoint.Publish(orderPlacedEvent);

        return "Compra iniciada, aguardando pagamento";
    }

    public async Task AdicionarJogoAoBiblioteca(Guid userId, Guid gameId)
    {
        var userGame = new UserGame(userId, gameId);

        await _userGameRepository.Adicionar(userGame);
        await _userGameRepository.Salvar();
    }

    public async Task<IEnumerable<UserGameResponseDTO>> ObterBiblioteca(Guid userId)
    {
        var userGames = await _userGameRepository.ObterPorUsuario(userId);
        return userGames.Select(ug => new UserGameResponseDTO
        {
            GameId = ug.GameId,
            Nome = ug.Game.Nome,
            Descricao = ug.Game.Descricao,
            Preco = ug.Game.Preco,
            DataCompra = ug.DataCompra
        }).ToList();
    }

    private static GameResponseDTO MapearParaGameResponse(Game game)
    {
        return new GameResponseDTO
        {
            Id = game.Id,
            Nome = game.Nome,
            Descricao = game.Descricao,
            Preco = game.Preco,
            DataCadastro = game.DataCadastro
        };
    }
}
