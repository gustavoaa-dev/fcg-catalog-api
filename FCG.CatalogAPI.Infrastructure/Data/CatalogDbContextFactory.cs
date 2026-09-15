using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FCG.CatalogAPI.Infrastructure.Data;

// Contexto usado apenas pelas ferramentas de design-time do EF (`dotnet ef`, ex.: migrations).
// A string de conexao vem da variavel de ambiente ConnectionStrings__DefaultConnection -- a mesma
// que a API usa no cluster -- porque o repositorio nao versiona credencial nenhuma.
public class CatalogDbContextFactory : IDesignTimeDbContextFactory<CatalogDbContext>
{
    public CatalogDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Defina a variavel de ambiente ConnectionStrings__DefaultConnection com a string de conexao do " +
                "SQL Server antes de rodar comandos do EF (ex.: dotnet ef migrations add). " +
                "O repositorio nao versiona credenciais.");
        }

        var optionsBuilder = new DbContextOptionsBuilder<CatalogDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new CatalogDbContext(optionsBuilder.Options);
    }
}
