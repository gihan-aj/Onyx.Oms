# Implementation Plan - Onyx.Oms VSA Structure

## Goal
Establish a foundational project structure for the Order Management System using Vertical Slice Architecture (VSA).

## Architecture Reference
> [!IMPORTANT]
> A detailed guide on how to implement features using VSA is available here:
> [VSA Architecture Guide](vsa_architecture_guide.md)

## Current Structure

### 1. `Onyx.Oms.Core` (Class Library)
**Purpose**: Holds the "Heart" of the system.
- **Domain Entities**: `Order`, `OrderItem`, `Customer`, etc.
- **Value Objects**: `Address`, `Money`, etc.
- **Interfaces**: Abstractions for infrastructure.
- **Dependencies**: None.

### 2. `Onyx.Oms.Infrastructure` (Class Library)
**Purpose**: Implementation of external concerns.
- **Dependencies**: `Onyx.Oms.Core`.

### 3. `Onyx.Oms.Web` (ASP.NET Core Web API)
**Purpose**: The entry point and host for Vertical Slices.
- **Features Folder**: Grouped by feature (e.g., `Features/Orders/CreateOrder`).
- **Dependencies**: `Onyx.Oms.Core`, `Onyx.Oms.Infrastructure`.

## Next Steps
1.  **Dependencies**: Install MediatR (or similar), FluentValidation, and EF Core packages.
2.  **Slices**: Create the `Features` folder structure in `Onyx.Oms.Web`.
3.  **Pipeline**: internal plumbing for MediatR behaviors (Logging, Validation).
