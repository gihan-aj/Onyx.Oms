using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Enums;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Domain.ValueObjects;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.FullfillmentTasks.IssuePurchaseOrder
{
    public class IssuePurchaseOrderHandler : ICommandHandler<IssuePurchaseOrderCommand>
    {
        private readonly IApplicationDbContext _context;
        public IssuePurchaseOrderHandler(IApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<Result> Handle(IssuePurchaseOrderCommand request, CancellationToken cancellationToken)
        {
            var procurementTask = await _context.FulfillmentTasks
                .FirstOrDefaultAsync(t => t.Id == request.ProcurementTaskId && t.Type == FulfillmentTaskType.Procurement, cancellationToken);

            if (procurementTask == null)
            {
                return Result.Failure(Error.NotFound("ProcurementTask.NotFound", "Procurement task not found."));
            }

            var variant = await _context.ProductVariants
                .FirstOrDefaultAsync(v => v.Id == procurementTask.ProductVariantId && v.IsActive == true, cancellationToken);
            if(variant == null)
                return Result.Failure(Error.NotFound("ProcurementTask.VarinatNotFound", "Product Variant not found."));

            var result = procurementTask.IssuePurchaseOrder(
                request.IssueQuantity, 
                request.PurchaseOrderNumber, 
                new Money(request.Cost.Amount, request.Cost.Currency));

            if (result.IsFailure)
                return Result.Failure(result.Error);

            var splitTask = result.Value;
            if(splitTask != null)
            {
                _context.FulfillmentTasks.Add(splitTask);
            }

            // Update incoming stock
            var variantUpdateResult = variant.AdjustIncomingStock(request.IssueQuantity);
            if (variantUpdateResult.IsFailure)
                return Result.Failure(variantUpdateResult.Error);

            // Order item status
            if (procurementTask.LinkedOrderItemId.HasValue)
            {
                var orderItem = await _context.OrderItems
                    .FirstOrDefaultAsync(oi => oi.Id == procurementTask.LinkedOrderItemId.Value, cancellationToken);

                if (orderItem != null)
                {
                    var order = await _context.Orders
                        .Include(o => o.Items)
                        .FirstOrDefaultAsync(o => o.Id == orderItem.OrderId, cancellationToken);

                    if (order != null)
                    {
                        order.UpdateStatus(Core.Domain.Enums.OrderStatus.Processing);
                        orderItem.UpdateStatus(Core.Domain.Enums.OrderItemStatus.Ordered);
                    }
                }

            }

            await _context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
    }
}
