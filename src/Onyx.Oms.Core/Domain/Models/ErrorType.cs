namespace Onyx.Oms.Core.Domain.Models;

public enum ErrorType
{
    Failure = 0,
    Validation = 1,
    NotFound = 2,
    Conflict = 3,
    Unauthorized = 4,
    Forbidden = 5,

    ThirdPartyService = 6,
    RateLimitExceeded = 7,
    ServiceUnavailable = 8,
    InvalidConfiguration = 9,
}
