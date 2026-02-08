# Onyx.Oms Vertical Slice Architecture Guide

This document serves as the implementation reference for the Onyx Order Management System. It details how to structure code, where to put logic, and how to maintain the Vertical Slice Architecture (VSA).

## Core Philosophy

> **"Minimize coupling between slices, maximize coupling within a slice."**

Each feature (e.g., "Create Order", "Update Customer Address") should be a self-contained unit. It should contain everything it needs to work:
- The API Endpoint
- The Request/Response models
- The Business Logic (Handler)
- The Validation Logic

## Project Structure & Responsibilities

### 1. `Onyx.Oms.Core` (The Domain)
**Responsibility**: Defines *what* the business is. It should have **zero dependencies** on external libraries aside from basic .NET types.
- **Entities**: Rich domain models with behavior (e.g., `Order.AddLineItem()`).
- **Value Objects**: Immutable types (e.g., `Money`, `Address`).
- **Domain Events**: Events that signify something important happened (e.g., `OrderPlacedEvent`).
- **Abstractions**: Interfaces for infrastructure (e.g., `IEmailSender`, `IOrderRepository`).
- **Exceptions**: Domain-specific exceptions (e.g., `OrderAlreadyShippedException`).

### 2. `Onyx.Oms.Infrastructure` (The Implementation)
**Responsibility**: Implements the abstractions defined in Core.
- **Persistence**: `DbContext`, Entity Framework configurations.
- **External Services**: Implementations of `IEmailSender`, File Storage, Payment Gateways.
- **Cross-Cutting Concerns**: Logging, Caching implementations (if external dependent).

### 3. `Onyx.Oms.Web` (The Application / Slices)
**Responsibility**: The entry point and the home for Vertical Slices.
- **`Features/` Folder**: The heart of the application logic.
    - Organized by **Module** -> **Feature**.
    - Example: `Features/Orders/CreateOrder/`

## Anatomy of a Feature Slice

A typical slice in `Onyx.Oms.Web/Features/Orders/CreateOrder/` contains:

1.  **`CreateOrderEndpoint.cs`**: 
    - Defines the HTTP route (POST /orders).
    - Maps the HTTP request to the `CreateOrderCommand`.
    - Returns the appropriate HTTP response.
    - *Tip: Use Minimal APIs or FastEndpoints for less boilerplate.*

2.  **`CreateOrderCommand.cs`**:
    - A simple DTO (Data Transfer Object) implementing `IRequest<Result>`.
    - Represents the *intent* to create an order.

3.  **`CreateOrderHandler.cs`**:
    - Implements `IRequestHandler<CreateOrderCommand, Result>`.
    - **Orchestrates** the logic:
        1.  Validates the command (implicitly or explicitly).
        2.  Loads necessary aggregates from repositories.
        3.  Calls domain methods on the aggregates (e.g., `order.Submit()`).
        4.  Saves changes via `DbContext` or Repository.
        5.  Returns a Result/Response.

4.  **`CreateOrderValidator.cs`**:
    - Uses FluentValidation to validate the incoming command.
    - keep validation logic separate from business logic.

5.  **`CreateOrderResponse.cs`** (Optional):
    - The DTO returned to the client.

## Rules of Engagement

1.  **Slices can talk to Core**: Slices use Entities and Interfaces from Core freely.
2.  **Slices do NOT talk to each other**: Use **Domain Events** or **Mediator** to decouple communication between slices.
    - *Bad*: `CreateOrderHandler` calls `SendEmailHandler` directly.
    - *Good*: `CreateOrderHandler` publishes `OrderCreatedEvent`. `SendEmailHandler` listens to it.
3.  **Infrastructure is injected**: Never instantiate Infrastructure classes directly. Use Dependency Injection to get the Interface defined in Core.
4.  **Keep Controllers Thin**: Or better yet, remove them entirely in favor of Minimal APIs or FastEndpoints to reduce "Bucket Brigade" coding.

## New Feature Checklist
- [ ] Create folder in `Features/[Module]/[FeatureName]`.
- [ ] Define `Command`/`Query` record.
- [ ] Implement `Handler`.
- [ ] Create `Endpoint` to expose via HTTP.
- [ ] Add `Validator`.
- [ ] Register in Dependency Injection (if not auto-discovered).
