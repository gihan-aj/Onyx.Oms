using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Orders.CancelOrder
{
    public class CancelOrderHandler : ICommandHandler<CancelOrderCommand>
    {
        private readonly IApplicationDbContext _context;

        public CancelOrderHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
        {
            var order = await _context.Orders
                .Include(o => o.Items)
                .Include(o => o.Payments)
                .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

            if (order == null)
                return Result.Failure(Error.NotFound("Order.NotFound", "Order not found."));

            if (order.PaymentStatus != Core.Domain.Enums.PaymentStatus.Unpaid)
                return Result.Failure(Error.Conflict("Order.PaymentSettling", "Cannot cancel order while payments are not settled."));

            // release reserved stock
            var allocatedItems = order.Items.Where(i => i.AllocatedQuantity > 0).ToList();
            var variantIds = allocatedItems.Select(i => i.ProductVariantId).Distinct().ToList();
            if (variantIds.Any())
            {
                var variants = await _context.ProductVariants
                    .Where(v => variantIds.Contains(v.Id))
                    .ToListAsync(cancellationToken);

                foreach (var orderItem in allocatedItems)
                {
                    var variant = variants.FirstOrDefault(v => v.Id == orderItem.ProductVariantId);
                    if (variant != null)
                    {
                        variant.ReleaseReservation(orderItem.AllocatedQuantity);
                    }
                }
            }

            // unlink any fulfillment tasks from the order
            var orderItemIds = order.Items.Select(i => i.Id).ToList();
            if (orderItemIds.Any())
            {
                var fulfillmentTasks = await _context.FulfillmentTasks
                    .Where(t => t.LinkedOrderItemId != null && orderItemIds.Contains(t.LinkedOrderItemId.Value))
                    .ToListAsync(cancellationToken);

                foreach (var task in fulfillmentTasks)
                {
                    task.UnlinkOrderItem();
                }
            }

            if (!string.IsNullOrWhiteSpace(request.Reason))
            {
                string regressNote = $"[{DateTimeOffset.UtcNow:g}] (UTC) System Note: Order Cancelled: {request.Reason}.";
                string updatedNotes = string.IsNullOrWhiteSpace(order.Notes)
                    ? regressNote
                    : order.Notes + Environment.NewLine + regressNote;

                order.UpdateNotes(updatedNotes);
            }

            var result = order.Cancel();
            if (result.IsFailure)
                return result;

            await _context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
    }
}
