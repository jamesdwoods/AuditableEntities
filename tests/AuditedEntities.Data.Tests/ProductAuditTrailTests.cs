using AuditedEntities.Data;
using AuditedEntities.Data.Sample;
using Microsoft.EntityFrameworkCore;

namespace AuditedEntities.Data.Tests;

/// <summary>
/// Exercises the full stack (EF Core migrations against a real SQL Server instance) to
/// verify that saving a <see cref="Product"/> automatically records
/// <see cref="ProductAuditTrailEntry"/> rows for its initial and changed field values.
/// </summary>
public class ProductAuditTrailTests : IDisposable
{
    private readonly AppDbContext _context;

    public ProductAuditTrailTests()
    {
        const string databaseName = "AuditedEntities_Test";
        var connectionString =
            $"Server=localhost;Database={databaseName};Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        _context = new AppDbContext(options);

        // Drop any database left behind by a previous run, then apply the real EF Core
        // migrations fresh. The database is deliberately left in place after the test
        // (see Dispose) so it can be inspected with SSMS/sqlcmd; the next run cleans it up.
        _context.Database.EnsureDeleted();
        _context.Database.Migrate();
    }

    [Fact]
    public void CreatingProductThenChangingPrice_RecordsAuditTrailForInitialAndChangedPrice()
    {
        var product = new Product
        {
            Id = 0, // database-generated identity; placeholder required by `required` member
            Name = "CargoWiseCloud.Orchestrator",
            Price = 0m,
        };

        _context.Products.Add(product);
        _context.SaveChanges();

        product.Price = 100m;
        _context.SaveChanges();

        var priceEntries = _context.ProductAuditTrailEntries
            .Where(e => e.EntityId == product.Id && e.FieldId == nameof(Product.Price))
            .OrderBy(e => e.Id)
            .ToList();

        Assert.Equal(2, priceEntries.Count);

        Assert.Null(priceEntries[0].OldValue);
        Assert.Equal("0", priceEntries[0].NewValue);
        Assert.Equal("system", priceEntries[0].UserId);

        Assert.Equal("0", priceEntries[1].OldValue);
        Assert.Equal("100", priceEntries[1].NewValue);
    }

    public void Dispose()
    {
        // Intentionally left in place for manual inspection (e.g. SSMS/sqlcmd against
        // "AuditedEntities_Test" on localhost). It is dropped and recreated at the
        // start of the next test run.
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
