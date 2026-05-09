using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Orders.AllocateOrderItemQuantity
{
    public class AllocateOrderItemQuantityHandler : ICommandHandler<AllocateOrderItemQuantityCommand>
    {
        private readonly IApplicationDbContext _context;

        public AllocateOrderItemQuantityHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result> Handle(AllocateOrderItemQuantityCommand request, CancellationToken cancellationToken)
        {
            var order = await _context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);
            if (order == null)
                return Result.Failure(Error.NotFound("Order.NotFound", "Order not found."));

            var orderItem = order.Items.FirstOrDefault(i => i.Id == request.OrderItemId);
            if(orderItem == null)
                return Result.Failure(Error.NotFound("OrderItem.NotFound", "Order Item not found."));

            if(orderItem.PendingQuantity < request.QuantityToAllocate)
                return Result.Failure(Error.NotFound("OrderItem.InavlidAllocation", "Allocating quantity cannot be larger than pending quantity."));

            var allocateQtyResult = orderItem.AllocateAvailableQuantity(request.QuantityToAllocate);
            if (allocateQtyResult.IsFailure)
                return Result.Failure(allocateQtyResult.Error);

            order.MarkIfReady();

            if(orderItem.Status == Core.Domain.Enums.OrderItemStatus.Ready)
            {
                // unlink any tasks
                var tasksToUnlink = await _context.FulfillmentTasks
                    .Where(t => t.LinkedOrderItemId != null && t.LinkedOrderItemId == request.OrderItemId)
                    .ToListAsync(cancellationToken);

                foreach (var task in tasksToUnlink)
                {
                    task.UnlinkOrderItem();
                }
            }
            
            await _context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
    }
}
