using MediatR;
using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Domain.Events;
using Onyx.Oms.Core.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Onyx.Oms.Web.Features.Orders.FulfillmentTaskCompleted
{
    public class FulfillmentTaskCompletedEventHandler : INotificationHandler<FulfillmentTaskCompletedEvent>
    {
        private readonly IApplicationDbContext _context;
        private readonly ILogger<FulfillmentTaskCompletedEventHandler> _logger;

        public FulfillmentTaskCompletedEventHandler(IApplicationDbContext context, ILogger<FulfillmentTaskCompletedEventHandler> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task Handle(FulfillmentTaskCompletedEvent notification, CancellationToken cancellationToken)
        {
            var orderItem = await _context.OrderItems
                .FirstOrDefaultAsync(oi => oi.Id == notification.OrderItemId, cancellationToken);

            if (orderItem == null)
            {
                _logger.LogWarning("OrderItem {OrderItemId} not found when handling FulfillmentTaskCompletedEvent for Task {TaskId}", notification.OrderItemId, notification.TaskId);
                return;
            }

            var order = await _context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == orderItem.OrderId, cancellationToken);

            if (order == null)
            {
                _logger.LogWarning("Order {OrderId} not found for OrderItem {OrderItemId}", orderItem.OrderId, notification.OrderItemId);
                return;
            }

            // Allocate up to the PendingQuantity
            int pendingQuantity = orderItem.PendingQuantity;
            int quantityToAllocate = Math.Min(notification.CompletedQuantity, pendingQuantity);

            if (quantityToAllocate > 0)
            {
                var allocateResult = orderItem.AllocateFromTask(quantityToAllocate);
                if (allocateResult.IsFailure)
                {
                    _logger.LogError("Failed to allocate {Quantity} to OrderItem {OrderItemId}: {Error}", quantityToAllocate, orderItem.Id, allocateResult.Error.Description);
                }
            }

            // Check if Order can be marked as ReadyToPack
            if (order.Status == OrderStatus.Confirmed || order.Status == OrderStatus.Processing)
            {
                bool allItemsReady = order.Items.All(i => i.Status == OrderItemStatus.Ready || i.Status == OrderItemStatus.Allocated);
                if (allItemsReady)
                {
                    var updateStatusResult = order.UpdateStatus(OrderStatus.ReadyToPack);
                    if (updateStatusResult.IsFailure)
                    {
                        _logger.LogWarning("Failed to auto-update order {OrderId} to ReadyToPack: {Error}", order.Id, updateStatusResult.Error.Description);
                    }
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
