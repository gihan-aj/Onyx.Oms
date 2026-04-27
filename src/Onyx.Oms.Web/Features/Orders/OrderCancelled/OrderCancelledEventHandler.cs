using MediatR;
using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Domain.Events;
using Onyx.Oms.Core.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Onyx.Oms.Web.Features.Orders.OrderCancelled
{
    public class OrderCancelledEventHandler : INotificationHandler<OrderCancelledEvent>
    {
        private readonly IApplicationDbContext _context;
        private readonly ILogger<OrderCancelledEventHandler> _logger;

        public OrderCancelledEventHandler(IApplicationDbContext context, ILogger<OrderCancelledEventHandler> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task Handle(OrderCancelledEvent notification, CancellationToken cancellationToken)
        {
            var order = await _context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == notification.OrderId, cancellationToken);

            if (order == null)
            {
                _logger.LogWarning("Order {OrderId} not found when handling OrderCancelledEvent", notification.OrderId);
                return;
            }

            var itemIds = order.Items.Select(i => i.Id).ToList();

            // 1. Release reserved stock
            var productVariantIds = order.Items.Select(i => i.ProductVariantId).Distinct().ToList();
            var variants = await _context.ProductVariants
                .Where(v => productVariantIds.Contains(v.Id))
                .ToDictionaryAsync(v => v.Id, cancellationToken);

            foreach (var item in order.Items)
            {
                if (item.AllocatedQuantity > 0 && variants.TryGetValue(item.ProductVariantId, out var variant))
                {
                    variant.ReleaseReservation(item.AllocatedQuantity);
                }
            }

            // 2. Cancel linked fulfillment tasks
            var linkedTasks = await _context.FulfillmentTasks
                .Where(t => t.LinkedOrderItemId != null && itemIds.Contains(t.LinkedOrderItemId.Value))
                .ToListAsync(cancellationToken);

            foreach (var task in linkedTasks)
            {
                if (task.Status == FulfillmentTaskStatus.Pending || task.Status == FulfillmentTaskStatus.InProgress)
                {
                    var cancelResult = task.Cancel();
                    if (cancelResult.IsFailure)
                    {
                        _logger.LogWarning("Failed to cancel FulfillmentTask {TaskId}: {Error}", task.Id, cancelResult.Error.Description);
                    }
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
