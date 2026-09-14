namespace FCG.CatalogAPI.Domain.Entities;

/// <summary>
/// Avaliação de um jogo por um usuário. Vive no MongoDB (persistência poliglota):
/// o schema é flexível de propósito (comentário opcional e lista de tags livre),
/// o que não caberia confortavelmente no modelo relacional do catálogo.
/// Sem atributos de persistência aqui: o mapeamento BSON fica na infraestrutura.
/// </summary>
public class Review
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid GameId { get; set; }
    public Guid UserId { get; set; }
    public int Nota { get; set; }
    public string? Comentario { get; set; }
    public List<string> Tags { get; set; } = new();
    public DateTime DataCriacao { get; set; }
    public DateTime DataAtualizacao { get; set; }
}
