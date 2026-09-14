using FCG.CatalogAPI.Domain.Entities;
using FCG.CatalogAPI.Domain.Interfaces;
using FCG.CatalogAPI.Infrastructure.Data;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace FCG.CatalogAPI.Infrastructure.Repositories;

public class MongoReviewRepository : IReviewRepository
{
    private const string NomeColecao = "avaliacoes";

    private readonly IMongoCollection<ReviewDocument> _colecao;
    private readonly ILogger<MongoReviewRepository> _logger;

    public MongoReviewRepository(IMongoDatabase database, ILogger<MongoReviewRepository> logger)
    {
        _colecao = database.GetCollection<ReviewDocument>(NomeColecao);
        _logger = logger;
    }

    public async Task InicializarAsync()
    {
        var chaves = Builders<ReviewDocument>.IndexKeys
            .Ascending(r => r.GameId)
            .Ascending(r => r.UserId);

        // Único: garante "uma avaliação por usuário e jogo" mesmo se duas requisições
        // do mesmo usuário chegarem juntas (o upsert sozinho não bastaria).
        await _colecao.Indexes.CreateOneAsync(
            new CreateIndexModel<ReviewDocument>(chaves, new CreateIndexOptions { Unique = true }));

        _logger.LogInformation("Indice unico (gameId, userId) garantido na colecao {Colecao}.", NomeColecao);
    }

    public async Task<(Review Avaliacao, bool Criada)> UpsertAsync(Review review)
    {
        var filtro = Builders<ReviewDocument>.Filter.Eq(r => r.GameId, review.GameId)
                     & Builders<ReviewDocument>.Filter.Eq(r => r.UserId, review.UserId);

        // Uma única operação atômica: cria na primeira vez (SetOnInsert preserva a data
        // original) e atualiza nas seguintes.
        var atualizacao = Builders<ReviewDocument>.Update
            .Set(r => r.Nota, review.Nota)
            .Set(r => r.Comentario, review.Comentario)
            .Set(r => r.Tags, review.Tags)
            .Set(r => r.DataAtualizacao, review.DataAtualizacao)
            .SetOnInsert(r => r.Id, review.Id)
            .SetOnInsert(r => r.DataCriacao, review.DataCriacao);

        var resultado = await _colecao.UpdateOneAsync(filtro, atualizacao, new UpdateOptions { IsUpsert = true });

        var criou = resultado.UpsertedId is not null;
        _logger.LogInformation("Avaliacao {Acao} para o jogo {GameId} pelo usuario {UserId}.",
            criou ? "criada" : "atualizada", review.GameId, review.UserId);

        // São duas operações de propósito. (1) FindOneAndUpdate não informa se houve
        // inserção, e é o UpsertedId do update acima que decide o 201/200. (2) Devolver o
        // objeto montado em memória dava contrato errado: numa atualização o documento
        // preserva o _id e a dataCriacao originais (SetOnInsert), então o corpo do PUT
        // responderia um id/instante que nunca existiram no store e contradiria o GET.
        // Reler o documento persistido é o que faz PUT e GET concordarem.
        var persistida = await _colecao.Find(filtro).FirstOrDefaultAsync();
        if (persistida is null)
            throw new InvalidOperationException(
                $"A avaliacao do jogo {review.GameId} pelo usuario {review.UserId} nao foi encontrada apos o upsert.");

        return (persistida.ParaEntidade(), criou);
    }

    public async Task<IEnumerable<Review>> ObterPorJogoAsync(Guid gameId)
    {
        var documentos = await _colecao
            .Find(Builders<ReviewDocument>.Filter.Eq(r => r.GameId, gameId))
            .SortByDescending(r => r.DataAtualizacao)
            .ToListAsync();

        return documentos.Select(d => d.ParaEntidade()).ToList();
    }

    public async Task<ReviewResumo> ObterResumoAsync(Guid gameId)
    {
        // O $group é montado como BsonDocument e a pipeline declara o serializador de saída
        // explicitamente: PipelineDefinition<TInput, TOutput>.Create(IEnumerable<BsonDocument>,
        // IBsonSerializer<TOutput>) é a sobrecarga documentada da API do driver.
        var estagios = new BsonDocument[]
        {
            new("$match", new BsonDocument("gameId", gameId.ToString())),
            new("$group", new BsonDocument
            {
                { "_id", BsonNull.Value },
                { "total", new BsonDocument("$sum", 1) },
                { "notaMedia", new BsonDocument("$avg", "$nota") }
            })
        };

        var pipeline = PipelineDefinition<ReviewDocument, BsonDocument>.Create(
            estagios,
            BsonDocumentSerializer.Instance);

        var resultado = await _colecao.Aggregate(pipeline).FirstOrDefaultAsync();
        if (resultado is null)
            return new ReviewResumo(0, null);

        return new ReviewResumo(
            resultado["total"].ToInt32(),
            Math.Round(resultado["notaMedia"].ToDouble(), 2));
    }
}
