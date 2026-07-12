namespace FCG.CatalogAPI.API.Middlewares;

public class ErroResponse
{
    public int StatusCode { get; set; }
    public string Mensagem { get; set; } = string.Empty;
    public string? Detalhe { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
