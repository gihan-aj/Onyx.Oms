using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Settings.AppSequences.UpdateAppSequenceValue;

public record UpdateAppSequenceValueCommand(string SequenceId, long NewValue) : ICommand;

public class UpdateAppSequenceValueHandler : ICommandHandler<UpdateAppSequenceValueCommand>
{
    private readonly IAppSequenceService _appSequenceService;

    public UpdateAppSequenceValueHandler(IAppSequenceService appSequenceService)
    {
        _appSequenceService = appSequenceService;
    }

    public async Task<Result> Handle(UpdateAppSequenceValueCommand request, CancellationToken cancellationToken)
    {
        var currentValue = await _appSequenceService.GetCurrentValueAsync(request.SequenceId, cancellationToken);

        if (currentValue.HasValue && request.NewValue < currentValue.Value)
        {
            return Result.Failure(Error.Validation(
                "AppSequence.InvalidNewValue", 
                $"The new sequence value ({request.NewValue}) cannot be less than the current value ({currentValue.Value})."));
        }

        return await _appSequenceService.UpdateCurrentValueAsync(request.SequenceId, request.NewValue, cancellationToken);
    }
}
