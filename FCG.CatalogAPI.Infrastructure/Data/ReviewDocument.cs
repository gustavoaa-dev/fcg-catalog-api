using FCG.CatalogAPI.Domain.Entities;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FCG.CatalogAPI.Infrastructure.Data;

/// <summary>
/// Representação do documento na coleção "avaliacoes". Os atributos do driver ficam
/// aqui, na infraestrutura, para o domínio continuar sem dependência de MongoDB.
/// Guid como string (BsonType.String) para o documento ficar legível no mongosh.
/// </summary>
public class ReviewDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }

    [BsonElement("gameId")]
    [BsonRepresentation(BsonType.String)]
    public Guid GameId { get; set; }

    [BsonElement("userId")]
    [BsonRepresentation(BsonType.String)]
    public Guid UserId { get; set; }

    [BsonElement("nota")]
    public int Nota { get; set; }

    [BsonElement("comentario")]
    [BsonIgnoreIfNull]
    public string? Comentario { get; set; }

    [BsonElement("tags")]
    public List<string> Tags { get; set; } = new();

    [BsonElement("dataCriacao")]
    public DateTime DataCriacao { get; set; }

    [BsonElement("dataAtualizacao")]
    public DateTime DataAtualizacao { get; set; }

    public static ReviewDocument De(Review review) => new()
    {
        Id = review.Id,
        GameId = review.GameId,
        UserId = review.UserId,
        Nota = review.Nota,
        Comentario = review.Comentario,
        Tags = review.Tags,
        DataCriacao = review.DataCriacao,
        DataAtualizacao = review.DataAtualizacao
    };

    public Review ParaEntidade() => new()
    {
        Id = Id,
        GameId = GameId,
        UserId = UserId,
        Nota = Nota,
        Comentario = Comentario,
        Tags = Tags,
        DataCriacao = DataCriacao,
        DataAtualizacao = DataAtualizacao
    };
}
