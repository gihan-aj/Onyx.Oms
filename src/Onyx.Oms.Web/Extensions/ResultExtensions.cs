using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Models;

namespace Onyx.Oms.Web.Extensions;

public static class ResultExtensions
{
    public static IResult ToProblemDetails(this Result result)
    {
        if (result.IsSuccess)
        {
            throw new InvalidOperationException("Can't convert success result to problem");
        }

        return Results.Problem(
            statusCode: GetStatusCode(result.Error.Type),
            title: GetTitle(result.Error.Type),
            type: GetType(result.Error.Type),
            extensions: new Dictionary<string, object?>
            {
                { "errors", new[] { result.Error } }
            });
    }

    public static IResult ToProblemDetails(this IValidationResult validationResult)
    {
         return Results.Problem(
            statusCode: StatusCodes.Status400BadRequest,
            title: "Validation Failed",
            type: "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            extensions: new Dictionary<string, object?>
            {
                { "errors", validationResult.Errors }
            });
    }

    private static int GetStatusCode(ErrorType errorType) =>
        errorType switch
        {
            ErrorType.Failure => StatusCodes.Status409Conflict,
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            ErrorType.ThirdPartyService => StatusCodes.Status502BadGateway,
            ErrorType.RateLimitExceeded => StatusCodes.Status429TooManyRequests,
            ErrorType.ServiceUnavailable => StatusCodes.Status503ServiceUnavailable,
            ErrorType.InvalidConfiguration => StatusCodes.Status500InternalServerError,
            _ => StatusCodes.Status500InternalServerError
        };

    private static string GetTitle(ErrorType errorType) =>
        errorType switch
        {
            ErrorType.Failure => "Error Occured",
            ErrorType.Validation => "Bad Request",
            ErrorType.NotFound => "Not Found",
            ErrorType.Conflict => "Conflict",
            ErrorType.Unauthorized => "Unauthorized",
            ErrorType.Forbidden => "Forbidden",
            ErrorType.ThirdPartyService => "External Service Error",
            ErrorType.RateLimitExceeded => "Rate Limit Exceeded",
            ErrorType.ServiceUnavailable => "Service Unavailable",
            ErrorType.InvalidConfiguration => "Configuration Error",
            _ => "Server Error"
        };
        
    private static string GetType(ErrorType errorType) =>
        errorType switch
        {
            ErrorType.Failure => "https://tools.ietf.org/html/rfc7231#section-6.5.8",
            ErrorType.Validation => "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            ErrorType.NotFound => "https://tools.ietf.org/html/rfc7231#section-6.5.4",
            ErrorType.Conflict => "https://tools.ietf.org/html/rfc7231#section-6.5.8",
            ErrorType.Unauthorized => "https://tools.ietf.org/html/rfc7231#section-6.5.2",
            ErrorType.Forbidden => "https://tools.ietf.org/html/rfc7231#section-6.5.3",
            ErrorType.ThirdPartyService => "https://tools.ietf.org/html/rfc7231#section-6.6.3",
            ErrorType.RateLimitExceeded => "https://tools.ietf.org/html/rfc6585#section-4",
            ErrorType.ServiceUnavailable => "https://tools.ietf.org/html/rfc7231#section-6.6.4",
            ErrorType.InvalidConfiguration => "https://tools.ietf.org/html/rfc7231#section-6.6.1",
            _ => "https://tools.ietf.org/html/rfc7231#section-6.6.1"
        };

    public static IResult Match<TValue>(this Result<TValue> result, Func<TValue, IResult> onSuccess, Func<Result, IResult> onFailure)
    {
        return result.IsSuccess ? onSuccess(result.Value) : onFailure(result);
    }
    
    public static IResult ToMinimalApiResult<TValue>(this Result<TValue> result)
    {
         if (result.IsSuccess)
         {
             return Results.Ok(result.Value);
         }

         if (result is IValidationResult validationResult)
         {
             return validationResult.ToProblemDetails();
         }

         return result.ToProblemDetails();
    }

    public static IResult ToMinimalApiResult(this Result result)
    {
         if (result.IsSuccess)
         {
             return Results.NoContent();
         }

         if (result is IValidationResult validationResult)
         {
             return validationResult.ToProblemDetails();
         }

         return result.ToProblemDetails();
    }
}
