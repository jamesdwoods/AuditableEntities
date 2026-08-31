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
    (`class Product : AuditedEntity<Product, int>`). Provides a `protected` (not publicly
    accessible) `AuditTrail` navigation collection and implements both
    `IAudited.GetAuditTrail` and the `GetAuditTrailEntries` convenience method for reading
    it, plus `AddAuditTrailEntry` for appending to it — so derived entities get audit
    trail support without re-implementing it or exposing the raw collection. Each entity
    implements the abstract `CreateAuditTrailEntry` factory method to construct its own
    concrete `AuditTrailEntry<TSelf, TKey>` subtype, so callers of `AddAuditTrailEntry`
    (e.g. `AppDbContext`) never need to reference that concrete type.
- **AuditedEntities.Data** — EF Core + SQL Server implementation:
  - `AppDbContext` — DbContext with `Products` and `ProductAuditTrailEntries` DbSets.
  - `Sample.Product` — example entity extending `AuditedEntity<Product, int>` (rather than
    implementing `IAudited` itself) and implementing `CreateAuditTrailEntry` to construct
    a `ProductAuditTrailEntry`. Since `AuditTrail` is `protected`, EF Core is configured to
    map it by navigation name (`HasMany<...>("AuditTrail")`) rather than a lambda.
  - `Sample.ProductAuditTrailEntry` — concrete `AuditTrailEntry<Product, int>`, mapped via
    EF Core table-per-hierarchy onto the same `ProductAuditTrailEntry` table as the
    abstract base type (it's the only concrete leaf, so the `Discriminator` column is
    always `"ProductAuditTrailEntry"`).
  - `AppDbContextFactory` — design-time factory so `dotnet ef` works without a startup project.
  - `Migrations/` — initial migration creating `Products` and `ProductAuditTrailEntry`
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

1. Create `MyEntity : AuditedEntity<MyEntity, TKey>` and implement `CreateAuditTrailEntry`
   to return a `new MyEntityAuditTrailEntry { ... }` — no need to implement `IAudited` or
   expose an `AuditTrail` collection yourself, it's inherited and stays encapsulated.
2. Create `MyEntityAuditTrailEntry : AuditTrailEntry<MyEntity, TKey>`.
3. Register the `MyEntity` `DbSet` and configure the relationship (via
   `HasMany<AuditTrailEntry<MyEntity, TKey>>("AuditTrail")`, since the collection is
   `protected`) + the base `AuditTrailEntry<MyEntity, TKey>` type (key, columns, table
   name) in `AppDbContext.OnModelCreating`, mirroring the `Product`/`ProductAuditTrailEntry`
   configuration (EF Core maps `MyEntityAuditTrailEntry` onto the same table via
   table-per-hierarchy).
4. Add a migration.
