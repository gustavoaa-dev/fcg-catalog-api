using FCG.CatalogAPI.Domain.Entities;
using FCG.CatalogAPI.Domain.Interfaces;
using FCG.CatalogAPI.Infrastructure.Cache;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Prometheus;
using System.Text.Json;

namespace FCG.CatalogAPI.Infrastructure.Repositories;

/// <summary>
/// Decorator de cache-aside sobre o repositório do catálogo. A listagem sem paginação
/// é a consulta onerosa do projeto; o Redis evita repeti-la a cada chamada.
/// Qualquer falha do cache degrada para o SQL: o Redis nunca derruba a API.
/// </summary>
public class CachedGameRepository : IGameRepository
{
    private const string ChaveTodos = "catalog:games:all";
    private static readonly TimeSpan Ttl = TimeSpan.FromSeconds(60);
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private static readonly Counter Hits = Metrics.CreateCounter("cache_hit", "Leituras do catalogo atendidas pelo Redis.");
    private static readonly Counter Misses = Metrics.CreateCounter("cache_miss", "Leituras do catalogo que foram ao SQL Server.");

    private readonly IGameRepository _interno;
    private readonly IDistributedCache _cache;
    private readonly ILogger<CachedGameRepository> _logger;
    private readonly List<Guid> _jogosAlterados = new();

    public CachedGameRepository(IGameRepository interno, IDistributedCache cache, ILogger<CachedGameRepository> logger)
    {
        _interno = interno;
        _cache = cache;
        _logger = logger;
    }

    private static string ChavePorId(Guid id) => $"catalog:game:{id}";

    public async Task<Game?> ObterPorId(Guid id)
    {
        var cacheado = await Ler<GameCacheItem>(ChavePorId(id));
        if (cacheado is not null)
            return cacheado.ParaEntidade();

        var game = await _interno.ObterPorId(id);
        if (game is not null)
            await Gravar(ChavePorId(id), GameCacheItem.De(game));

        return game;
    }

    public async Task<IEnumerable<Game>> ObterTodos()
    {
        var cacheado = await Ler<List<GameCacheItem>>(ChaveTodos);
        if (cacheado is not null)
            return cacheado.Select(i => i.ParaEntidade()).ToList();

        var games = (await _interno.ObterTodos()).ToList();
        await Gravar(ChaveTodos, games.Select(GameCacheItem.De).ToList());

        return games;
    }

    public async Task Adicionar(Game game)
    {
        await _interno.Adicionar(game);
        _jogosAlterados.Add(game.Id);
    }

    public async Task Remover(Game game)
    {
        await _interno.Remover(game);
        _jogosAlterados.Add(game.Id);
    }

    public async Task Salvar()
    {
        await _interno.Salvar();

        // Sem isto o POST/DELETE de jogo ficaria até 60s mentindo para quem lê.
        var chaves = new List<string> { ChaveTodos };
        chaves.AddRange(_jogosAlterados.Select(ChavePorId));
        _jogosAlterados.Clear();

        foreach (var chave in chaves)
            await RemoverChave(chave);
    }

    private async Task<T?> Ler<T>(string chave) where T : class
    {
        try
        {
            var json = await _cache.GetStringAsync(chave);
            if (string.IsNullOrEmpty(json))
            {
                Misses.Inc();
                return null;
            }

            var valor = JsonSerializer.Deserialize<T>(json, Json);
            if (valor is null)
            {
                Misses.Inc();
                return null;
            }

            Hits.Inc();
            return valor;
        }
        catch (Exception ex)
        {
            // Redis fora do ar (ou conteúdo corrompido): segue para o SQL.
            Misses.Inc();
            _logger.LogWarning(ex, "Falha ao ler a chave {Chave} do Redis; seguindo para o SQL Server.", chave);
            return null;
        }
    }

    private async Task Gravar<T>(string chave, T valor)
    {
        try
        {
            var json = JsonSerializer.Serialize(valor, Json);
            await _cache.SetStringAsync(chave, json, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = Ttl
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Falha ao gravar a chave {Chave} no Redis; a resposta segue sem cache.", chave);
        }
    }

    private async Task RemoverChave(string chave)
    {
        try
        {
            await _cache.RemoveAsync(chave);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Falha ao invalidar a chave {Chave} no Redis.", chave);
        }
    }
}
