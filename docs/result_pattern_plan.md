# Result Pattern & CQRS Abstractions Implementation Plan

## Goal
Establish a robust error handling and messaging foundation using a `Result` pattern and strongly-typed CQRS interfaces.

## 1. Domain Errors (`Onyx.Oms.Core/Domain/Models`)
We need a standard way to represent errors.

```csharp
public record Error(string Code, string Description)
{
    public static readonly Error None = new(string.Empty, string.Empty);
    public static readonly Error NullValue = new("Error.NullValue", "The specified result value is null.");
    // Helper methods like implicit conversion...
}
```

## 2. Result Pattern (`Onyx.Oms.Core/Common/Models`)
Wrapper for operation outcomes.

```csharp
public class Result
{
    // Success/Failure logic
    // Properties: IsSuccess, IsFailure, Error
}

public class Result<TValue> : Result
{
    // Value property
    // Accessor methods
}
```

## 3. CQRS Interfaces (`Onyx.Oms.Core/Common/Interfaces` or `Messaging`)
Abstracting MediatR to enforce the Result pattern.

```csharp
// ICommand returns Result by default
public interface ICommand : IRequest<Result> { }

// ICommand<TResponse> returns Result<TResponse>
public interface ICommand<TResponse> : IRequest<Result<TResponse>> { }

// IQuery<TResponse> returns Result<TResponse>
public interface IQuery<TResponse> : IRequest<Result<TResponse>> { }
```

## 4. Helper Interface (`IDomainEvent`)
If we want to standardize domain events later.
