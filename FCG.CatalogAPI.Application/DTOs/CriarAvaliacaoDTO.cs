namespace FCG.CatalogAPI.Application.DTOs;

/// <summary>
/// Entrada da avaliação. Não existe campo de usuário de propósito: o autor vem
/// sempre do claim "Id" do token, nunca do corpo da requisição.
/// </summary>
public class CriarAvaliacaoDTO
{
    public int Nota { get; set; }
    public string? Comentario { get; set; }
    public List<string>? Tags { get; set; }
}
