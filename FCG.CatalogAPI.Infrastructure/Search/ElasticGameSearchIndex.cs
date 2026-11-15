using System.Net.Http.Json;
using System.Text.Json;
using FCG.CatalogAPI.Domain.Entities;
using FCG.CatalogAPI.Domain.Interfaces;

namespace FCG.CatalogAPI.Infrastructure.Search;

/// <summary>
/// Índice no Elasticsearch via HTTP. Sem cliente oficial de propósito: são três
/// chamadas e o System.Text.Json já está no projeto.
/// </summary>
public class ElasticGameSearchIndex : IGameSearchIndex
{
    private const string Indice = "games";
    private readonly HttpClient _http;
    private readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web);

    public ElasticGameSearchIndex(HttpClient http) => _http = http;

    public async Task IndexarAsync(Game game)
    {
        var resposta = await _http.PutAsJsonAsync($"/{Indice}/_doc/{game.Id}", new
        {
            nome = game.Nome,
            descricao = game.Descricao,
            preco = game.Preco,
            dataCadastro = game.DataCadastro
        }, _json);

        resposta.EnsureSuccessStatusCode();
    }

    public async Task RemoverAsync(Guid id)
    {
        var resposta = await _http.DeleteAsync($"/{Indice}/_doc/{id}");

        // 404 = já não estava no índice; é o estado desejado.
        if (resposta.StatusCode != System.Net.HttpStatusCode.NotFound)
            resposta.EnsureSuccessStatusCode();
    }

    public async Task ReindexarAsync(IEnumerable<Game> games)
    {
        foreach (var game in games)
            await IndexarAsync(game);
    }

    public async Task<IReadOnlyList<Game>> BuscarAsync(string termo)
    {
        var corpo = new
        {
            size = 20,
            query = new
            {
                multi_match = new
                {
                    query = termo,
                    fields = new[] { "nome^3", "descricao" },
                    fuzziness = "AUTO"
                }
            }
        };

        var resposta = await _http.PostAsJsonAsync($"/{Indice}/_search", corpo, _json);
        resposta.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(await resposta.Content.ReadAsStringAsync());

        // O id vem do _id do documento: o _source não o carrega.
        return doc.RootElement.GetProperty("hits").GetProperty("hits").EnumerateArray()
            .Select(hit =>
            {
                var fonte = hit.GetProperty("_source");
                return new Game(
                    Guid.Parse(hit.GetProperty("_id").GetString()!),
                    fonte.GetProperty("nome").GetString()!,
                    fonte.GetProperty("descricao").GetString()!,
                    fonte.GetProperty("preco").GetDecimal(),
                    fonte.GetProperty("dataCadastro").GetDateTime());
            })
            .ToList();
    }
}
