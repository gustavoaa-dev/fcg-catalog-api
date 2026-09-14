namespace FCG.CatalogAPI.Domain.Entities;

/// <summary>
/// Resultado agregado das avaliações de um jogo, calculado no próprio MongoDB
/// (pipeline de agregação) em vez de trazer os documentos para somar na aplicação.
/// </summary>
public record ReviewResumo(int Total, double? NotaMedia);
