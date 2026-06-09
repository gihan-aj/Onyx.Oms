using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Enums;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.FullfillmentTasks.CancelProductionTask;

public class CancelProductionTaskHandler : ICommandHandler<CancelProductionTaskCommand>
{
    private readonly IApplicationDbContext _context;

    public CancelProductionTaskHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(CancelProductionTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await _context.FulfillmentTasks
            .FirstOrDefaultAsync(t => t.Id == request.ProductionTaskId, cancellationToken);
        
        if (task is null)
            return Result.Failure(Error.NotFound("ProductionTask.NotFound", "Production task not found."));

        if (task.Type != FulfillmentTaskType.Production)
            return Result.Failure(Error.Validation("Task.InvalidType", "The specified task is not a production task."));

        var variant = await _context.ProductVariants
                .FirstOrDefaultAsync(v => v.Id == task.ProductVariantId && v.IsActive == true, cancellationToken);
        if (variant == null)
            return Result.Failure(Error.NotFound("ProductionTask.VarinatNotFound", "Product Variant not found."));

        var result = task.Cancel();
        if (result.IsFailure)
            return Result.Failure(result.Error);

        // Update incoming stock
        var variantUpdateResult = variant.AdjustIncomingStock(-result.Value);
        if (variantUpdateResult.IsFailure)
            return Result.Failure(variantUpdateResult.Error);

        if (task.LinkedOrderItemId.HasValue)
        {
            var orderItem = await _context.OrderItems
                .Include(oi => oi.Order)
                .FirstOrDefaultAsync(oi => oi.Id == task.LinkedOrderItemId.Value, cancellationToken);
            if (orderItem != null)
            {
                if (orderItem.Status != OrderItemStatus.Allocated || orderItem.Status != OrderItemStatus.Ready)
                {
                    var anyOtherTasksLInked = await _context.FulfillmentTasks
                        .Where(t => t.LinkedOrderItemId == orderItem.Id &&
                            (t.Status == FulfillmentTaskStatus.Pending || t.Status == FulfillmentTaskStatus.InProgress))
                        .ToListAsync(cancellationToken);

                    if (!anyOtherTasksLInked.Any())
                    {
                        orderItem.RevertToPending();
                    }
                    else if (!anyOtherTasksLInked.Any(t => t.Status == FulfillmentTaskStatus.InProgress) && orderItem.Status == OrderItemStatus.InProduction)
                    {
                        orderItem.RevertInProduction();
                    }

                }
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
