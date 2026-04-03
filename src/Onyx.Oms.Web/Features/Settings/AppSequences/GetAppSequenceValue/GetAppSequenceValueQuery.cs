using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Settings.AppSequences.GetAppSequenceValue;

public record GetAppSequenceValueQuery(string Prefix) : IQuery<long>;

public class GetAppSequenceValueHandler : IQueryHandler<GetAppSequenceValueQuery, long>
{
    private readonly IAppSequenceService _appSequenceService;

    public GetAppSequenceValueHandler(IAppSequenceService appSequenceService)
    {
        _appSequenceService = appSequenceService;
    }

    public async Task<Result<long>> Handle(GetAppSequenceValueQuery request, CancellationToken cancellationToken)
    {
        var currentValue = await _appSequenceService.GetCurrentValueAsync(request.Prefix, cancellationToken);
        
        if (currentValue == null)
        {
            return Result.Failure<long>(Error.NotFound("AppSequence.NotFound", $"App sequence with ID '{request.Prefix}' was not found."));
        }

        return Result<long>.Success(currentValue.Value);
    }
}
