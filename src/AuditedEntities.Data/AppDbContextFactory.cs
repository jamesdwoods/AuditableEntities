using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AuditedEntities.Data;

/// <summary>
/// Enables "dotnet ef migrations add/remove" and "dotnet ef database update" to run
/// from the command line without a startup project providing DI-configured options.
/// Uses a local SQL Server / LocalDB connection string for design-time only; replace
/// with your real connection string (e.g. from configuration) at runtime.
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlServer(
            "Server=(localdb)\\mssqllocaldb;Database=AuditedEntities;Trusted_Connection=True;MultipleActiveResultSets=true");

        return new AppDbContext(optionsBuilder.Options);
    }
}
