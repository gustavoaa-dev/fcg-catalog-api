namespace FCG.CatalogAPI.Domain.Entities;

public class Game
{
    public Guid Id { get; private set; }
    public string Nome { get; private set; }
    public string Descricao { get; private set; }
    public decimal Preco { get; private set; }
    public DateTime DataCadastro { get; private set; }
    public ICollection<UserGame> Usuarios { get; private set; }

    public Game(string nome, string descricao, decimal preco)
    {
        Id = Guid.NewGuid();
        Nome = nome;
        Descricao = descricao;
        Preco = preco;
        DataCadastro = DateTime.UtcNow;
        Usuarios = new List<UserGame>();
    }

    /// <summary>
    /// Reidrata um jogo já persistido, preservando o Id original. É usado pelo cache
    /// (que guarda o objeto serializado); para criar um jogo novo continua valendo
    /// apenas o construtor de três parâmetros, que gera um Id.
    /// </summary>
    public Game(Guid id, string nome, string descricao, decimal preco, DateTime dataCadastro)
    {
        Id = id;
        Nome = nome;
        Descricao = descricao;
        Preco = preco;
        DataCadastro = dataCadastro;
        Usuarios = new List<UserGame>();
    }
}
