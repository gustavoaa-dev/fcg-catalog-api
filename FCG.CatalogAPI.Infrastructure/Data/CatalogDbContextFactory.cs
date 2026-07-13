using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FCG.CatalogAPI.Infrastructure.Data;

public class CatalogDbContextFactory : IDesignTimeDbContextFactory<CatalogDbContext>
{
    public CatalogDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<CatalogDbContext>();
        optionsBuilder.UseSqlServer("Server=127.0.0.1;Database=FCG_Catalog;User Id=sa;Password=Fcg2024Test!;Encrypt=False;TrustServerCertificate=True");

        return new CatalogDbContext(optionsBuilder.Options);
    }
}
