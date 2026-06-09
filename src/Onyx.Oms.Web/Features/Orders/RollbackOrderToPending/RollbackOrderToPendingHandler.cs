using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Orders.RollbackOrderToPending
{
    public class RollbackOrderToPendingHandler : ICommandHandler<RollbackOrderToPendingCommand>
    {
        private readonly IApplicationDbContext _context;

        public RollbackOrderToPendingHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result> Handle(RollbackOrderToPendingCommand request, CancellationToken cancellationToken)
        {
            var order = await _context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

            if (order == null)
                return Result.Failure(Error.NotFound("Order.NotFound", "Order not found."));

            var revertResult = order.RevertToPending(request.Reason);
            if (revertResult.IsFailure)
                return revertResult;

            var variantIds = order.Items.Select(oi => oi.ProductVariantId).Distinct().ToList();
            var variants = await _context.ProductVariants
                .Where(v => variantIds.Contains(v.Id))
                .ToListAsync(cancellationToken);

            var orderItemIds = order.Items.Select(oi => oi.Id).Distinct().ToList();
            var tasksToUnlink = await _context.FulfillmentTasks
                .Where(t => t.LinkedOrderItemId != null && orderItemIds.Contains(t.LinkedOrderItemId.Value))
                .ToListAsync(cancellationToken);

            foreach (var task in tasksToUnlink)
            {
                task.UnlinkOrderItem();
            }

            foreach (var item in order.Items)
            {
                var allocatedQty = item.AllocatedQuantity;
                if(allocatedQty > 0)
                {
                    var variant = variants.FirstOrDefault(v => v.Id == item.ProductVariantId);
                    if (variant != null)
                        variant.ReleaseReservation(allocatedQty);
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
    }

}
