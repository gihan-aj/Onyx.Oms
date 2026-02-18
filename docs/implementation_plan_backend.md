# Implementation Plan - Backend Authentication & User Mirror

This document outlines the implementation plan for the **Backend Authentication, User Mirroring, and Dynamic Permissions** in `Onyx.Oms.Web`.

## Architecture Decisions

1.  **Vertical Slice Architecture**: Features organized by domain.
2.  **Minimal APIs**: Use `IEndpoint` pattern (Scalar/FastEndpoints style) instead of Controllers.
3.  **Options Pattern**: Use `IOptions<AuthenticationOptions>` for configuration.
4.  **JSON Storage**:
    - `Role.Permissions`: Stored as a JSON list of strings (e.g., `["Order.View", "Order.Refund"]`).
    - `AppUser.Roles`: Stored as a JSON list of strings (e.g., `["Admin", "Manager"]`).
    - **Reasoning**: Reduces join tables (`UserRoles`, `RolePermissions`) for simpler, faster read-heavy authorization checks.
5.  **Manual Migrations**: The developer will manually create and apply migrations after entity changes.

## Proposed Changes

### 1. Core Domain (`Onyx.Oms.Core`)

#### [NEW] `AppUser` Entity
- **Path**: `src/Onyx.Oms.Core/Domain/Entities/AppUser.cs`
- **Properties**:
  - `Id` (int, PK)
  - `IdentityUserId` (string, required, unique index) - Links to IdP
  - `Email` (string)
  - `DisplayName` (string)
  - `Roles` (List<string>) - JSON stored
  - `LastLoginUtc` (DateTime?)
- Inherits from `AuditableEntity`.

#### [NEW] `Role` Entity
- **Path**: `src/Onyx.Oms.Core/Domain/Entities/Role.cs`
- **Properties**:
  - `Id` (int, PK)
  - `Name` (string, unique)
  - `Description` (string)
  - `Permissions` (List<string>) - JSON stored
- Inherits from `AuditableEntity`.

#### [NEW] `IUserMirrorService` Interface
- **Path**: `src/Onyx.Oms.Core/Common/Interfaces/IUserMirrorService.cs`
- **Methods**: `SyncUserAsync`, `SyncRolesAsync`.

### 2. Infrastructure (`Onyx.Oms.Infrastructure`)

#### [NEW] `AuthenticationOptions`
- **Path**: `src/Onyx.Oms.Infrastructure/Identity/AuthenticationOptions.cs`
- **Properties**: `Authority`, `Audience`, `MetadataUrl`.

#### [MODIFY] `AppDbContext`
- **Path**: `src/Onyx.Oms.Infrastructure/Persistence/AppDbContext.cs`
- Add `DbSet<AppUser>`, `DbSet<Role>`.
- Configure JSON conversion for `Roles` and `Permissions` properties using `HasConversion`.

#### [NEW] `UserMirrorMiddleware`
- **Path**: `src/Onyx.Oms.Infrastructure/Middleware/UserMirrorMiddleware.cs`
- **Logic**:
  1.  Intercept authenticated requests.
  2.  Check for `sub` claim.
  3.  Check if user exists in `AppUsers` (Cache -> DB).
  4.  If missing, create new `AppUser` (JIT Provisioning).
  5.  Update `LastLoginUtc`.

#### [MODIFY] `DependencyInjection`
- **Path**: `src/Onyx.Oms.Infrastructure/DependencyInjection.cs`
- Register `AuthenticationOptions` from `appsettings.json`.
- Configure JWT Bearer Auth.

### 3. Web API (`Onyx.Oms.Web`)

#### [MODIFY] `appsettings.json`
- Add `Authentication` section.

#### [NEW] `AuthEndpoints`
- **Path**: `src/Onyx.Oms.Web/Endpoints/Auth/AuthEndpoints.cs`
- **Endpoints**:
  - `POST /api/auth/sync`: Force sync user roles (Optional, mainly for testing/admin).

#### [MODIFY] `Program.cs`
- Register `UserMirrorMiddleware`.
- Use Authentication & Authorization.

## Verification

### Automated Tests
- Unit tests for `UserMirrorMiddleware` logic.
- Integration tests for `AppUser` creation on request.

### Manual Steps
1.  **Configure**: Update `appsettings.json` with local IdP details.
2.  **Authenticate**: Get a token from IdP.
3.  **Call API**: Hit a protected endpoint (e.g., `GET /api/products`).
4.  **Verify DB**: Check `AppUsers` table for the new user.
5.  **Verify Roles**: Check `Roles` column has correct JSON data.
