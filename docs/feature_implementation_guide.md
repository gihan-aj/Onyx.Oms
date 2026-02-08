# Feature Implementation Guide

Follow this guide to implement new features (Vertical Slices) in `Onyx.Oms.Web` to ensure consistency.

## 1. Folder Structure
Create a new folder in `Web/Features/[FeatureName]/[ActionName]`.
*Example*: `Web/Features/Customers/CreateCustomer`

## 2. Define the Command/Query
Create a `record` implementing `ICommand` or `IQuery<TResponse>`.
-   **Commands**: modifying state (Create, Update, Delete). Return `Result` or `Result<Id>`.
-   **Queries**: reading state. Return `Result<Dto>`.
-   **Inputs**: Include all necessary fields. Use specific types (Guid, etc.).

```csharp
public record CreateCustomerCommand(string Name, string Email) : ICommand<Guid>;
```

## 3. Implement the Handler
Create a class implementing `IRequestHandler<TRequest, TResponse>`.
-   **Inject**: `IApplicationDbContext` (not DbContext directly if possible).
-   **Logic**:
    1.  Validate business rules (uniqueness, existence) -> Return `Result.Failure` if failed.
    2.  Interact with Domain Entity (factory methods, behaviors).
    3.  Save Changes (`_context.SaveChangesAsync`).
    4.  Return `Result.Success`.
-   **Queries**: Used `AsNoTracking()` for read performance.

## 4. Create the Validator (Optional but Recommended)
Create a class inheriting `AbstractValidator<TRequest>`.
-   Validate inputs (NotEmpty, MaxLength, Email format).
-   *Note*: Business logic validation (e.g. "Email already taken") usually goes in the Handler, structural validation goes here.

## 5. Create the Endpoint
Create a class implementing `IEndpoint`.
-   **Route**: Use RESTful naming (e.g., `api/v1/customers`).
-   **Params**: Use `[FromBody]`, `[AsParameters]` (for queries), or route params `{id}`.
-   **Response**: Use `result.ToMinimalApiResult()`.
    -   Success: `200 OK` (with data) or `204 No Content`.
    -   Error: `ProblemDetails` (400, 404, 409).
-   **Documentation**: Add `.WithSummary()` and `.WithDescription()`.

```csharp
public class CreateCustomerEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/v1/customers", ...)
           .WithSummary("Create Customer");
    }
}
```

## 6. Checklist
-   [ ] **Command/Query**: Defined?
-   [ ] **Handler**: Implemented logic & error handling?
-   [ ] **Validator**: Input rules defined?
-   [ ] **Endpoint**: Mapped & documented?
-   [ ] **Registered**: Auto-discovery handles this, just build & run!
