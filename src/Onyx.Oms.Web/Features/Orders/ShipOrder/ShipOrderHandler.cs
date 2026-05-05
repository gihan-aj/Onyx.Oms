using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Orders.ShipOrder
{
    public class ShipOrderHandler : ICommandHandler<ShipOrderCommand>
    {
        private readonly IApplicationDbContext _context;

        public ShipOrderHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result> Handle(ShipOrderCommand request, CancellationToken cancellationToken)
        {
            var order = await _context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

            if (order == null)
                return Result.Failure(Error.NotFound("Order.NotFound", "Order not found."));

            bool courierExists = await _context.Couriers.AnyAsync(c => c.Id == request.CourierId && c.IsActive, cancellationToken);
            if (!courierExists)
                return Result.Failure(Error.NotFound("Courier.NotFound", "Courier not found or inactive."));

            var result = order.Ship(request.CourierId, request.TrackingNumber);
            if (result.IsFailure)
                return result;

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

            await _context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
    }
}
