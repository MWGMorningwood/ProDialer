using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ProDialer.Functions.Data;

/// <summary>
/// Design-time factory for ProDialerDbContext.
/// This is used by Entity Framework tools (e.g., dotnet ef migrations) when running at design time.
/// </summary>
public class ProDialerDbContextFactory : IDesignTimeDbContextFactory<ProDialerDbContext>
{
    public ProDialerDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ProDialerDbContext>();
        
        // Use Azure SQL Database connection string for design-time operations
        // This matches the production connection string from local.settings.json
        optionsBuilder.UseSqlServer("Server=tcp:bezsql.database.windows.net,1433;Initial Catalog=db-prodialer;Encrypt=True;Connection Timeout=30;Authentication=Active Directory Default;");
        
        return new ProDialerDbContext(optionsBuilder.Options);
    }
}
