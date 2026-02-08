# Testing Plan - Onyx.Oms

## Goal
Ensure high code quality and reliability by implementing a comprehensive Unit Testing strategy for the `Courier` feature and future modules.

## 1. Project Setup
-   **Project Name**: `Onyx.Oms.UnitTests`
-   **Location**: `tests/Onyx.Oms.UnitTests` (Create `tests` folder at root)
-   **Type**: xUnit Test Project (`dotnet new xunit`)
-   **References**:
    -   `Onyx.Oms.Core` (for Domain/Application logic)
    -   `Onyx.Oms.Web` (for Validators/Endpoints if needed, usually prefer separate Integration Tests, but for VSA, handlers might be in Web)
    -   *Note*: Since we use VSA, handlers are in `Web`. We must reference `Web` to test Handlers.

## 2. Dependencies
-   `xunit`: Test runner.
-   `FluentAssertions`: Readable assertions (`result.Should().BeSuccess()`).
-   `NSubstitute`: Mocking dependencies (`IApplicationDbContext`).
-   `Microsoft.EntityFrameworkCore.InMemory` (Optional, for simple DbContext substitute if NSubstitute is too complex for DbSets, but mocking `IDbSet` is usually preferred or using the "In-Memory" provider for a quick integration-like unit test).
    *Decided*: Use `NSubstitute` for `IApplicationDbContext` to mock `DbSet`.

## 3. Test Scope
### A. Domain Tests (`Core.Domain.Entities`)
-   **Courier**:
    -   `Create`: Should return Success/Failure based on logic.
    -   `UpdateDetails`: Should update fields.
    -   `Activate/Deactivate`: Should change state.

### B. Application Tests (`Web.Features`)
-   **CreateCourierHandler**:
    -   Should return Success when valid.
    -   Should return Failure when Name is duplicate (Mock `AnyAsync`).
-   **GetCourierByIdHandler**:
    -   Should return DTO when exists.
    -   Should return NotFound when missing.
-   **UpdateCourierHandler**:
    -   Should check uniqueness if name changes.
    -   Should update properties.

### C. Validation Tests (`Validators`)
-   **CreateCourierValidator**:
    -   Should fail on empty Name.
    -   Should fail on long strings.

## 4. Folder Structure (Matching VSA)
```
tests/Onyx.Oms.UnitTests/
├── Domain/
│   └── Couriers/
│       └── CourierTests.cs
├── Features/
│   └── Couriers/
│       ├── CreateCourier/
│       │   ├── CreateCourierHandlerTests.cs
│       │   └── CreateCourierValidatorTests.cs
│       └── ...
└── Common/
    └── Mocks/ (Helpers for mocking DbContext)
```

## 5. Next Steps
1.  Create `tests` folder and Project.
2.  Add NuGets.
3.  Implement Domain Tests.
4.  Implement Handler Tests.
