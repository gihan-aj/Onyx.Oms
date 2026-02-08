# Project Setup Guide (Onyx.Oms Template)

This guide documents the steps taken to set up the **Onyx.Oms** solution. Use this as a checklist when starting a new project with the same architecture.

## 1. Solution & Project Creation
1.  **Create Solution**: `dotnet new sln -n ProjectName`
2.  **Create Core (Class Library)**: `dotnet new classlib -n ProjectName.Core`
3.  **Create Infrastructure (Class Library)**: `dotnet new classlib -n ProjectName.Infrastructure`
4.  **Create Web (Web API)**: `dotnet new webapi -n ProjectName.Web`
5.  **Add to Solution**: `dotnet sln add **/*.csproj`

## 2. Dependencies & References
### References
-   `Web` -> `Core`
-   `Web` -> `Infrastructure`
-   `Infrastructure` -> `Core`

### NuGet Packages
-   **Core**:
    -   `MediatR`
    -   `FluentValidation`
    -   `Microsoft.EntityFrameworkCore` (Abstractions)
-   **Infrastructure**:
    -   `Microsoft.EntityFrameworkCore.SqlServer` (or Postgres, etc.)
-   **Web**:
    -   `Microsoft.EntityFrameworkCore.Design`
    -   `Asp.Versioning.Http`
    -   `Scalar.AspNetCore` (for API Docs)

## 3. Core Building Blocks (The Plumbing)
### Common Models
-   `Result<T>` / `Error` / `ErrorType`: Functional error handling.
-   `Entity` / `AuditableEntity`: Base classes for DDD.
-   `PagedResult<T>` / `PagedRequest`: Standard pagination.
-   `ICommand` / `IQuery`: CQRS abstractions (MediatR).
-   `IApplicationDbContext`: Interface for DbContext.

### Behaviors (Pipeline)
-   `LoggingBehavior<TRequest, TResponse>`: Logs all requests/responses/errors.
-   `ValidationBehavior<TRequest, TResponse>`: Auto-validates using FluentValidation.

## 4. Infrastructure Setup
-   **Persistence**:
    -   `AppDbContext`: Inherits `DbContext`, implements `IApplicationDbContext`.
    -   `AuditableEntityInterceptor`: Automatically sets `CreatedBy`/`LastModifiedBy`.
    -   `Configurations`: Use `IEntityTypeConfiguration<T>` for fluent API.
-   **DependencyInjection**:
    -   `AddCore()`: Registers MediatR, Behaviors, Validators.
    -   `AddInfrastructure()`: Registers DbContext, Interceptors.

## 5. Web API Setup
-   **Minimal APIs**:
    -   `IEndpoint` interface: `void MapEndpoint(IEndpointRouteBuilder app)`.
    -   `EndpointExtensions`: Scans assembly to auto-register attributes.
    -   `MapGroup`/`WithApiVersionSet`: Standard versioning setup.
-   **Program.cs**:
    -   Call `AddCore()` and `AddInfrastructure()`.
    -   Call `app.MapEndpoints()`.
    -   Configure `Scalar` and `API Versioning`.

---
**Done!** The foundation is ready for Feature Slices.
