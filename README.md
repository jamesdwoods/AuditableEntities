# AuditableEntities

A small C# solution demonstrating reusable base types for building auditable,
Entity Framework Core-managed SQL Server entities.

## Projects

- **AuditableEntities.Domain** — abstractions, no EF/SQL dependency:
  - `Entity<TKey>` — abstract base class for entities, parameterized on the primary key type.
  - `AuditTrailEntry<TEntity, TKey>` — abstract base class for a single audited field
    change (`EntityId`, `Timestamp`, `UserId`, `FieldId`, `OldValue`, `NewValue`),
    constrained so `TEntity : Entity<TKey>` (conceptually `AuditTrailEntry<Entity<TKey>>`).
  - `IAuditable<TEntity, TKey>` — implemented by an entity (or service) that can return
    its `AuditTrailEntry<TEntity, TKey>` history.
  - `AuditedEntity<TSelf, TKey>` — optional convenience base class combining `Entity<TKey>`
    and `IAuditable<TSelf, TKey>` via the curiously-recurring-template pattern
    (`class Product : AuditedEntity<Product, int>`). Provides the `AuditTrail`
    navigation collection and implements both `IAuditable.GetAuditTrail` and the
    `GetAuditTrailEntries` convenience method, so derived entities get audit trail
    support without re-implementing it.
- **AuditableEntities.Data** — EF Core + SQL Server implementation:
  - `AppDbContext` — DbContext with `Products` and `ProductAuditTrailEntries` DbSets.
  - `Sample.Product` — example entity (`Entity<int>`) that implements `IAuditable<Product, int>`
    directly, exposing its audit history as the EF navigation collection `AuditTrail`
    (no separate repository/reader class needed — just `.Include(p => p.AuditTrail)`).
  - `Sample.ProductAuditTrailEntry` — concrete `AuditTrailEntry<Product, int>`.
  - `AppDbContextFactory` — design-time factory so `dotnet ef` works without a startup project.
  - `Migrations/` — initial migration creating `Products` and `ProductAuditTrailEntries`
    tables with a FK from the audit table to `Products.Id`.
  - `AppDbContext.SaveChanges`/`SaveChangesAsync` automatically create a
    `ProductAuditTrailEntry` per added/changed scalar field on tracked `Product`s
    (attributed to `AppDbContext.CurrentUserId`, default `"system"`).
- **AuditableEntities.Data.Tests** — xUnit tests that run EF Core migrations against a
  real SQL Server LocalDB instance (a fresh, disposable database per test) and verify
  that creating a product and changing its price produces the expected audit trail.

## Working with migrations

```powershell
cd src\AuditableEntities.Data
dotnet ef migrations add <Name>
dotnet ef database update   # requires a reachable SQL Server / LocalDB instance
```

The connection string used for design-time tooling is in `AppDbContextFactory.cs`
(LocalDB by default) — supply your real connection string via `DbContextOptionsBuilder`
at runtime (e.g. from configuration/DI) instead.

## Extending to a new entity

1. Create `MyEntity : Entity<TKey>, IAuditable<MyEntity, TKey>` with an
   `ICollection<MyEntityAuditTrailEntry> AuditTrail` navigation property.
2. Create `MyEntityAuditTrailEntry : AuditTrailEntry<MyEntity, TKey>`.
3. Register both `DbSet`s and configure the relationship in `AppDbContext.OnModelCreating`,
   mirroring the `Product`/`ProductAuditTrailEntry` configuration.
4. Add a migration.
