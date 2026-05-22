namespace Onyx.Oms.Core.Domain.Models;

public record Error
{
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.Failure);
    public static readonly Error NullValue = new("Error.NullValue", "The specified result value is null.", ErrorType.Failure);

    public Error(string code, string description, ErrorType type)
    {
        Code = code;
        Description = description;
        Type = type;
    }

    public string Code { get; }
    public string Description { get; }
    public ErrorType Type { get; }

    public static Error Failure(string code, string description) => new(code, description, ErrorType.Failure);
    public static Error NotFound(string code, string description) => new(code, description, ErrorType.NotFound);
    public static Error Conflict(string code, string description) => new(code, description, ErrorType.Conflict);
    public static Error Validation(string code, string description) => new(code, description, ErrorType.Validation);
    public static Error Unauthorized(string code, string description) => new(code, description, ErrorType.Unauthorized);
    public static Error Forbidden(string code, string description) => new(code, description, ErrorType.Forbidden);

    public static Error ThirdPartyService(string code, string description)
        => new(code, description, ErrorType.ThirdPartyService);

    public static Error RateLimitExceeded(string code, string description)
        => new(code, description, ErrorType.RateLimitExceeded);

    public static Error ServiceUnavailable(string code, string description)
        => new(code, description, ErrorType.ServiceUnavailable);

    public static Error InvalidConfiguration(string code, string description)
        => new(code, description, ErrorType.InvalidConfiguration);
}
