namespace FCG.CatalogAPI.Application.DTOs;

public class ReviewResponseDTO
{
    public Guid Id { get; set; }
    public Guid GameId { get; set; }
    public Guid UsuarioId { get; set; }
    public int Nota { get; set; }
    public string? Comentario { get; set; }
    public List<string> Tags { get; set; } = new();
    public DateTime DataCriacao { get; set; }
    public DateTime DataAtualizacao { get; set; }
}
