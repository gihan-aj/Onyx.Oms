using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.FullfillmentTasks.CompleteBatch
{
    public class CompleteBatchHandler : ICommandHandler<CompleteBatchCommand>
    {
        private readonly IApplicationDbContext _context;

        public CompleteBatchHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result> Handle(CompleteBatchCommand request, CancellationToken cancellationToken)
        {
            var variant = await _context.ProductVariants
                .FirstOrDefaultAsync(v => v.Id == request.ProductVariantId && v.IsActive == true, cancellationToken);
            if (variant == null)
                return Result.Failure(Error.NotFound("Variant.NotFound", "Product Variant not found."));

            var tasks = await _context.FulfillmentTasks
                .Where(t => t.ProductVariantId == request.ProductVariantId 
                    && (t.Status == Core.Domain.Enums.FulfillmentTaskStatus.Pending || t.Status == Core.Domain.Enums.FulfillmentTaskStatus.InProgress))
                .ToListAsync(cancellationToken);
            if (!tasks.Any())
                return Result.Failure(Error.NotFound("Tasks.NotFound", "Tasks are not found for the specific product."));

            foreach(var task in tasks)
            {
                // if user try to complete a pending task or not all requested quantity hasnt been started...
                int quantityToComplete = task.RequestedQuantity - task.CompletedQuantity;

                var incomingStockAdjustment = quantityToComplete > task.StartedQuantity
                    ? task.StartedQuantity
                    : quantityToComplete;

                var result = task.MarkReady(quantityToComplete);
                if (result.IsFailure)
                    return Result.Failure(result.Error);

                // Update stock     
                if (incomingStockAdjustment > 0)
                {
                    var incomingStockResult = variant.AdjustIncomingStock(-incomingStockAdjustment);
                    if (incomingStockResult.IsFailure)
                        return Result.Failure(incomingStockResult.Error);
                }

                var stockResult = variant.AdjustStock(quantityToComplete);
                if (stockResult.IsFailure)
                    return Result.Failure(stockResult.Error);

                // Allocate to order if an order item is linked
                if (request.AllocateToOrders
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
                    int quantityToAllocate = Math.Min(quantityToComplete, pendingQuantity);

                    if (quantityToAllocate > 0)
                    {
                        var allocateResult = orderItem.AllocateFromTask(quantityToAllocate);
                        if (allocateResult.IsFailure)
                            return Result.Failure(allocateResult.Error);
                    }

                    // Check and mark if Order can be marked as ReadyToPack
                    order.MarkIfReady();

                    // reserve the allocated quantity
                    var reserveStockResult = variant.ReserveStockFromTask(quantityToAllocate);
                    if (reserveStockResult.IsFailure)
                        return Result.Failure(reserveStockResult.Error);
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
    }
}
