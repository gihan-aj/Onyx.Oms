# Onyx.Oms

> A multi-tenant Order Management System backend built with .NET 10, vertical slice architecture, and CQRS — designed for a real apparel business, architected like a SaaS product from day one.

Onyx.Oms is the API behind [Onyx.Oms.Client](https://github.com/gihan-aj/Onyx.Oms.Client), handling the full order lifecycle for a clothing business that sells through Facebook and WhatsApp: catalog and stock management, order confirmation, production/procurement fulfillment for out-of-stock items, manual payment tracking, and shipping.

It started as the backend for a single-business desktop app, but was deliberately over-engineered with multi-tenancy from the start as a way to learn how real SaaS tenant isolation works — with the intention of eventually running this same codebase as a hosted product for multiple businesses.

## Domain & business logic

- **Catalog**: hierarchical categories, products, and product variants (size/color) with independent stock tracking.
- **Orders**: full lifecycle from `Pending` → `Confirmed` → `Processing` → `Ready to Pack` → `Packed` → `Shipped` → `Delivered` → `Completed`, plus return/loss states (`Returned to Sender`, `Lost in Transit`, `Delivery Failed`, etc.).
- **Stock-flexible confirmation**: orders can be confirmed even with insufficient stock — shortages are tracked as Fulfillment Tasks (Production or Procurement) rather than blocking the sale.
- **Payments**: manual payment recording with COD-aware balance calculation (shipping fee is deducted from balance due on COD orders) and configurable per-gateway fee rates.
- **Financial reporting**: expense tracking against configurable categories and monthly financial summaries.

See [`docs/business-logic.md`](docs/business-logic.md) for the full domain reference, including status definitions and business rules.

## Architecture

**Vertical Slice Architecture** — features live together instead of being split across horizontal layers:

```
src/
├── Onyx.Oms.Core/            # Domain entities, value objects, interfaces
├── Onyx.Oms.Infrastructure/  # EF Core DbContext, interceptors, migrations
└── Onyx.Oms.Web/
    └── Features/
        ├── Customers/
        │   ├── CreateCustomer/
        │   └── GetCustomer/
        ├── Orders/
        │   ├── ConfirmOrder/
        │   └── ...
        └── ...
```

Each feature folder (e.g. `Features/Customers/CreateCustomer`) is self-contained: a MediatR command/query, its handler, an optional FluentValidation validator, and a minimal API endpoint all live side by side. There's no separate Application layer — CQRS handlers sit directly in `Onyx.Oms.Web`, working against `Core` domain entities via `Infrastructure`.

**Stack:**
- **.NET 10**, Minimal APIs with Swagger/OpenAPI
- **MediatR** for CQRS (commands mutate, queries read with `AsNoTracking()`)
- **FluentValidation** for structural input validation (business-rule validation, like uniqueness checks, lives in the handler)
- **Result pattern** for error handling instead of exceptions for expected failures — endpoints map `Result`/`Result<T>` to proper HTTP responses (200/204 on success, `ProblemDetails` with 400/404/409 on failure)
- **API versioning** via `Asp.Versioning`
- **EF Core** with SQL Server

## Multi-tenancy

Onyx.Oms enforces tenant isolation at multiple layers rather than relying on developers to remember a `WHERE TenantId = ...` clause:

- **`TenantSecurityInterceptor`** — an EF Core `SaveChanges` interceptor that requires any entity implementing `IMustHaveTenant` to have a tenant ID set, and verifies it matches the current user's active tenant before allowing the write.
- **Tenant resolution middleware** — determines the caller's active tenant on every request.
- **Global query filter** — automatically scopes all `IMustHaveTenant` entities to the active tenant, unless an explicit `TenantSecurityBypass` is in effect for a trusted internal operation.
- **Impersonation** — users with the impersonation permission can supply a target tenant ID via request header, which becomes their active tenant for that request (used for platform-admin support scenarios).
- **Permission-based authorization** is implemented throughout, even though the current deployment is a single-tenant desktop install per business — laying the groundwork for the SaaS version.

This was built specifically as a learning exercise in tenant isolation patterns before taking the product cloud-hosted and multi-tenant for real (see [Onyx.IdP](#) and the SaaS roadmap below).

## Getting started

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server (LocalDB or full instance)

### Installation

```bash
git clone <repository-url>
cd Onyx.Oms
dotnet restore
```

Update the connection string in `src/Onyx.Oms.Web/appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=your-server;Database=OnyxOms;User Id=your-user;Password=your-password;"
}
```

Apply migrations and run:

```bash
dotnet ef database update --project src/Onyx.Oms.Infrastructure --startup-project src/Onyx.Oms.Web
dotnet run --project src/Onyx.Oms.Web
```

## Testing

```bash
dotnet test tests/Onyx.Oms.UnitTests
dotnet test tests/Onyx.Oms.IntegrationTests
```

## Related repositories

- [`Onyx.Oms.Client`](https://github.com/gihan-aj/Onyx.Oms.Client) — the WinUI 3 desktop client that bundles this API and Onyx.IdP into a single installable app
- [`Onyx.IdP`](https://github.com/gihan-aj/Onyx.IdP) — the OpenID Connect identity provider used for authentication and tenant management

## Roadmap

- Migrating to a cloud-hosted multi-tenant SaaS product (Clerk for auth/organizations, Neon for Postgres, Railway for API hosting, Vercel for frontend)
- Courier API integration, once a courier with a documented API is adopted
- Deeper Meta/WhatsApp Business API automation, which requires the cloud-hosted version to support webhooks

## License

MIT
