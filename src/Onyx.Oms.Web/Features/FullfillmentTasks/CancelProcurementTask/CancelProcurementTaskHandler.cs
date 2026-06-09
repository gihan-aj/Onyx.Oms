using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Enums;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.FullfillmentTasks.CancelProcurementTask;

public class CancelProcurementTaskHandler : ICommandHandler<CancelProcurementTaskCommand>
{
    private readonly IApplicationDbContext _context;

    public CancelProcurementTaskHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(CancelProcurementTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await _context.FulfillmentTasks
            .FirstOrDefaultAsync(t => t.Id == request.ProcurementTaskId, cancellationToken);
        
        if (task is null)
            return Result.Failure(Error.NotFound("ProcurementTask.NotFound", "Procurement task not found."));

        if (task.Type != FulfillmentTaskType.Procurement)
            return Result.Failure(Error.Validation("Task.InvalidType", "The specified task is not a procurement task."));

        var variant = await _context.ProductVariants
                .FirstOrDefaultAsync(v => v.Id == task.ProductVariantId && v.IsActive == true, cancellationToken);
        if (variant == null)
            return Result.Failure(Error.NotFound("ProcurementTask.VarinatNotFound", "Product Variant not found."));

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
            if(orderItem != null)
            {
                if(orderItem.Status != OrderItemStatus.Allocated || orderItem.Status != OrderItemStatus.Ready)
                {
                    var anyOtherTasksLInked = await _context.FulfillmentTasks
                        .Where(t => t.LinkedOrderItemId == orderItem.Id && 
                            (t.Status == FulfillmentTaskStatus.Pending || t.Status == FulfillmentTaskStatus.InProgress))
                        .ToListAsync(cancellationToken);

                    if (!anyOtherTasksLInked.Any())
                    {
                        orderItem.RevertToPending();
                    }
                    else if (!anyOtherTasksLInked.Any(t => t.Status == FulfillmentTaskStatus.InProgress) && orderItem.Status == OrderItemStatus.Ordered)
                    {
                        orderItem.RevertOrdered();
                    }
                    
                }
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
