using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Enums;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.FullfillmentTasks.ScrapProcurementTask;

public class ScrapProcurementTaskHandler : ICommandHandler<ScrapProcurementTaskCommand>
{
    private readonly IApplicationDbContext _context;

    public ScrapProcurementTaskHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(ScrapProcurementTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await _context.FulfillmentTasks
            .FirstOrDefaultAsync(t => t.Id == request.ProcurementTaskId, cancellationToken);
        
        if (task is null)
            return Result.Failure(Error.NotFound("ProcurementTask.NotFound", "Procurement task not found."));

        if (task.Type != FulfillmentTaskType.Procurement)
            return Result.Failure(Error.Validation("Task.InvalidType", "The specified task is not a procurement task."));

        var result = task.MarkScrapped(request.QuantityToScrap);
        if (result.IsFailure)
            return Result.Failure(result.Error);

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
