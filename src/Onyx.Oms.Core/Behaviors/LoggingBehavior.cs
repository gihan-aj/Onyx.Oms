using MediatR;
using Microsoft.Extensions.Logging;
using Onyx.Oms.Core.Common.Models;

namespace Onyx.Oms.Core.Behaviors;

public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : Result
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        _logger.LogInformation("Processing Request: {Name} {@Request}", requestName, request);

        var response = await next();

        if (response.IsFailure)
        {
            _logger.LogError("Request Failure: {Name} {@Error} {@Request}",
                requestName,
                response.Error,
                request);
        }
        else
        {
            _logger.LogInformation("Completed Request: {Name}", requestName);
        }

        return response;
    }
}
