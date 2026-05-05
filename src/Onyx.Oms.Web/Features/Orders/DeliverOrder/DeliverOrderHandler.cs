using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Orders.DeliverOrder
{
    public class DeliverOrderHandler : ICommandHandler<DeliverOrderCommand>
    {
        private readonly IApplicationDbContext _context;

        public DeliverOrderHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result> Handle(DeliverOrderCommand request, CancellationToken cancellationToken)
        {
            var order = await _context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

            if (order == null)
                return Result.Failure(Error.NotFound("Order.NotFound", "Order not found."));

            bool courierExists = await _context.Couriers.AnyAsync(c => c.Id == order.CourierId && c.IsActive, cancellationToken);
            if (!courierExists)
                return Result.Failure(Error.NotFound("Courier.NotFound", "Courier not found or inactive."));

            if(order.Status < Core.Domain.Enums.OrderStatus.Shipped)
            {
                // Deduct stock physically
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
                            // deducts both StockOnHand and ReservedQuantity
                            variant.MarkShipped(orderItem.AllocatedQuantity);
                        }
                    }
                }
            }

            var result = order.Deliver();
            if (result.IsFailure)
                return result;

            await _context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
    }
}
