using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FCG.CatalogAPI.Infrastructure.Data;

public class CatalogDbContextFactory : IDesignTimeDbContextFactory<CatalogDbContext>
{
    public CatalogDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<CatalogDbContext>();
        optionsBuilder.UseSqlServer("Server=localhost;Database=FCG_Catalog;Trusted_Connection=True;TrustServerCertificate=True");

        return new CatalogDbContext(optionsBuilder.Options);
    }
}
