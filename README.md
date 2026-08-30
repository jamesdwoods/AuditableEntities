# AuditedEntities

A small C# solution demonstrating reusable base types for building auditable,
Entity Framework Core-managed SQL Server entities.

## Projects

- **AuditedEntities.Domain** — abstractions, no EF/SQL dependency:
  - `Entity<TKey>` — abstract base class for entities, parameterized on the primary key
    type (`TKey : IComparable<TKey>`, so entities can be ordered/sorted by key).
  - `AuditTrailEntry<TEntity, TKey>` — abstract base class for a single audited field
    change (`EntityId`, `Timestamp`, `UserId`, `FieldId`, `OldValue`, `NewValue`),
    constrained so `TEntity : Entity<TKey>` (conceptually `AuditTrailEntry<Entity<TKey>>`).
  - `IAudited<TEntity, TKey>` — implemented by an entity (or service) that can return
    its `AuditTrailEntry<TEntity, TKey>` history.
  - `AuditedEntity<TSelf, TKey>` — optional convenience base class combining `Entity<TKey>`
    and `IAudited<TSelf, TKey>` via the curiously-recurring-template pattern
    (`class Product : AuditedEntity<Product, int>`). Provides the `AuditTrail`
    navigation collection and implements both `IAudited.GetAuditTrail` and the
    `GetAuditTrailEntries` convenience method, so derived entities get audit trail
    support without re-implementing it.
- **AuditedEntities.Data** — EF Core + SQL Server implementation:
  - `AppDbContext` — DbContext with `Products` and `ProductAuditTrailEntries` DbSets.
  - `Sample.Product` — example entity extending `AuditedEntity<Product, int>` (rather than
    implementing `IAudited` itself), so it gets the `AuditTrail` navigation collection and
    `GetAuditTrail`/`GetAuditTrailEntries` for free — just `.Include(p => p.AuditTrail)`.
  - `Sample.ProductAuditTrailEntry` — concrete `AuditTrailEntry<Product, int>`, mapped via
    EF Core table-per-hierarchy onto the same `ProductAuditTrailEntries` table as the
    abstract base type (it's the only concrete leaf, so the `Discriminator` column is
    always `"ProductAuditTrailEntry"`).
  - `AppDbContextFactory` — design-time factory so `dotnet ef` works without a startup project.
  - `Migrations/` — initial migration creating `Products` and `ProductAuditTrailEntries`
    tables with a FK from the audit table to `Products.Id`.
  - `AppDbContext.SaveChanges`/`SaveChangesAsync` automatically create a
    `ProductAuditTrailEntry` per added/changed scalar field on tracked `Product`s
    (attributed to `AppDbContext.CurrentUserId`, default `"system"`).
- **AuditedEntities.Data.Tests** — xUnit tests that run EF Core migrations against a
  real SQL Server LocalDB instance (a fresh, disposable database per test) and verify
  that creating a product and changing its price produces the expected audit trail.

## Working with migrations

```powershell
cd src\AuditedEntities.Data
dotnet ef migrations add <Name>
dotnet ef database update   # requires a reachable SQL Server / LocalDB instance
```

The connection string used for design-time tooling is in `AppDbContextFactory.cs`
(LocalDB by default) — supply your real connection string via `DbContextOptionsBuilder`
at runtime (e.g. from configuration/DI) instead.

## Extending to a new entity

1. Create `MyEntity : AuditedEntity<MyEntity, TKey>` — no need to implement `IAudited`
   or `AuditTrail` yourself, it's inherited.
2. Create `MyEntityAuditTrailEntry : AuditTrailEntry<MyEntity, TKey>`.
3. Register the `MyEntity` `DbSet` and configure the relationship + the base
   `AuditTrailEntry<MyEntity, TKey>` type (key, columns, table name) in
   `AppDbContext.OnModelCreating`, mirroring the `Product`/`ProductAuditTrailEntry`
   configuration (EF Core maps `MyEntityAuditTrailEntry` onto the same table via
   table-per-hierarchy).
4. Add a migration.
