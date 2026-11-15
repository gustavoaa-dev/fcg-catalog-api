using System.Net;
using FCG.CatalogAPI.Infrastructure.Search;

namespace FCG.CatalogAPI.Tests;

public class ElasticGameSearchIndexTests
{
    private sealed class FakeHandler : HttpMessageHandler
    {
        public string? Corpo { get; private set; }
        public Uri? Uri { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
        {
            Uri = request.RequestUri;
            Corpo = request.Content is null ? null : await request.Content.ReadAsStringAsync(token);

            var resposta = request.Method == HttpMethod.Get
                ? "{}"
                : "{\"hits\":{\"hits\":[{\"_id\":\"11111111-1111-1111-1111-111111111111\"," +
                  "\"_source\":{\"nome\":\"Jogo de Ação\",\"descricao\":\"Aventura\",\"preco\":49.9," +
                  "\"dataCadastro\":\"2026-11-15T12:00:00Z\"}}]}}";

            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(resposta) };
        }
    }

    [Fact]
    public async Task BuscarAsync_DeveEnviarFuzzyComPesoNoNome()
    {
        var handler = new FakeHandler();
        var sut = new ElasticGameSearchIndex(new HttpClient(handler) { BaseAddress = new Uri("http://es:9200") });

        await sut.BuscarAsync("jgoo");

        Assert.Contains("\"fuzziness\":\"AUTO\"", handler.Corpo);
        Assert.Contains("nome^3", handler.Corpo);
        Assert.Contains("_search", handler.Uri!.ToString());
    }

    [Fact]
    public async Task BuscarAsync_DeveDevolverJogoComIdDoDocumento()
    {
        var sut = new ElasticGameSearchIndex(new HttpClient(new FakeHandler()) { BaseAddress = new Uri("http://es:9200") });

        var jogos = await sut.BuscarAsync("jogo");

        var jogo = Assert.Single(jogos);
        Assert.Equal(Guid.Parse("11111111-1111-1111-1111-111111111111"), jogo.Id);
        Assert.Equal("Jogo de Ação", jogo.Nome);
        Assert.Equal(49.9m, jogo.Preco);
    }

    [Fact]
    public async Task RemoverAsync_DeveAceitar404ComoSucesso()
    {
        var handler = new NotFoundHandler();
        var sut = new ElasticGameSearchIndex(new HttpClient(handler) { BaseAddress = new Uri("http://es:9200") });

        var excecao = await Record.ExceptionAsync(() => sut.RemoverAsync(Guid.NewGuid()));

        Assert.Null(excecao);
    }

    private sealed class NotFoundHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
    }
}
