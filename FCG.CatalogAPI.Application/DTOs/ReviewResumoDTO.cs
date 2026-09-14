namespace FCG.CatalogAPI.Application.DTOs;

public class ReviewResumoDTO
{
    public Guid JogoId { get; set; }
    public int Total { get; set; }
    public double? NotaMedia { get; set; }
}
