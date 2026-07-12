namespace FCG.CatalogAPI.Application.DTOs;

public class UserGameResponseDTO
{
    public Guid GameId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public decimal Preco { get; set; }
    public DateTime DataCompra { get; set; }
}
