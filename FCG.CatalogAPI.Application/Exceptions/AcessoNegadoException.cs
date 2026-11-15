namespace FCG.CatalogAPI.Application.Exceptions;

/// <summary>Usuário autenticado tentando agir sobre recurso de outro.</summary>
public class AcessoNegadoException : Exception
{
    public AcessoNegadoException(string mensagem) : base(mensagem)
    {
    }
}
