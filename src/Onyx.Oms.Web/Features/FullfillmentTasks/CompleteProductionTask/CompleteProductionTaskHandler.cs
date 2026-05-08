using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Enums;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.FullfillmentTasks.CompleteProductionTask;

public class CompleteProductionTaskHandler : ICommandHandler<CompleteProductionTaskCommand>
{
    private readonly IApplicationDbContext _context;

    public CompleteProductionTaskHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(CompleteProductionTaskCommand request, CancellationToken cancellationToken)
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

        // if user try to complete a pending task or not all requested quantity hasnt been started...
        var incomingStockAdjustment = request.QuantityToComplete > task.StartedQuantity
            ? task.StartedQuantity
            : request.QuantityToComplete;

        var result = task.MarkReady(request.QuantityToComplete);
        if (result.IsFailure)
            return Result.Failure(result.Error);

        // Update stock     
        if(incomingStockAdjustment > 0)
        {
            var incomingStockResult = variant.AdjustIncomingStock(-incomingStockAdjustment);
            if (incomingStockResult.IsFailure)
                return Result.Failure(incomingStockResult.Error);
        }
   
        var stockResult = variant.AdjustStock(request.QuantityToComplete);
        if (stockResult.IsFailure)
            return Result.Failure(stockResult.Error);

        // Allocate to order if an order item is linked
        if (request.allocateToOrder.HasValue
            && request.allocateToOrder.Value
            && task.LinkedOrderItemId.HasValue
            && task.LinkedOrderItemId.Value != Guid.Empty)
        {
            var orderItem = await _context.OrderItems
                .FirstOrDefaultAsync(oi => oi.Id == task.LinkedOrderItemId, cancellationToken);
            if (orderItem == null)
                return Result.Failure(Error.NotFound("OrderItem.NotFound", "The order item that linked with the procurement task is not found"));

            var order = await _context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == orderItem.OrderId, cancellationToken);
            if (order == null)
                return Result.Failure(Error.NotFound("OrderItem.NotFound", "The order that linked with the procurement task is not found"));

            // Allocate up to the PendingQuantity
            int pendingQuantity = orderItem.PendingQuantity;
            int quantityToAllocate = Math.Min(request.QuantityToComplete, pendingQuantity);

            if (quantityToAllocate > 0)
            {
                var allocateResult = orderItem.AllocateFromTask(quantityToAllocate);
                if (allocateResult.IsFailure)
                    return Result.Failure(allocateResult.Error);
            }

            // Check and mark if Order can be marked as ReadyToPack
            order.Ready();

            // reserve the allocated quantity
            var reserveStockResult = variant.ReserveStockFromTask(quantityToAllocate);
            if (reserveStockResult.IsFailure)
                return Result.Failure(reserveStockResult.Error);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
