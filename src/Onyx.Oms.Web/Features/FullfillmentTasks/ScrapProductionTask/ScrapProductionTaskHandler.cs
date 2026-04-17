using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Enums;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.FullfillmentTasks.ScrapProductionTask;

public class ScrapProductionTaskHandler : ICommandHandler<ScrapProductionTaskCommand>
{
    private readonly IApplicationDbContext _context;

    public ScrapProductionTaskHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(ScrapProductionTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await _context.FulfillmentTasks
            .FirstOrDefaultAsync(t => t.Id == request.ProductionTaskId, cancellationToken);
        
        if (task is null)
            return Result.Failure(Error.NotFound("ProductionTask.NotFound", "Production task not found."));

        if (task.Type != FulfillmentTaskType.Production)
            return Result.Failure(Error.Validation("Task.InvalidType", "The specified task is not a production task."));

        var result = task.MarkScrapped(request.QuantityToScrap);
        if (result.IsFailure)
            return Result.Failure(result.Error);

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
