using System.ComponentModel.DataAnnotations;

namespace FCG.CatalogAPI.Application.DTOs;

public class CriarGameDTO
{
    [Required]
    [MinLength(3)]
    [MaxLength(100)]
    public string Nome { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string Descricao { get; set; } = string.Empty;

    [Required]
    [Range(0.01, 9999.99)]
    public decimal Preco { get; set; }
}
