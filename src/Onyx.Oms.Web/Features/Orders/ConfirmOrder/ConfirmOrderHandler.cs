using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Orders.ConfirmOrder
{
    public class ConfirmOrderHandler : ICommandHandler<ConfirmOrderCommand>
    {
        private readonly IApplicationDbContext _context;

        public ConfirmOrderHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result> Handle(ConfirmOrderCommand request, CancellationToken cancellationToken)
        {
            var order = await _context.Orders
                .Include(o => o.Items)
                .Include(o => o.Payments) // Payments are needed to verify TotalPaid
                .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

            if (order == null)
                return Result.Failure(Error.NotFound("Order.NotFound", "Order not found."));

            var variantIds = order.Items.Select(i => i.ProductVariantId).Distinct().ToList();
            var variants = await _context.ProductVariants
                .Where(v => variantIds.Contains(v.Id))
                .ToListAsync(cancellationToken);

            foreach(var orderItem in order.Items)
            {
                var variant = variants.FirstOrDefault(v => v.Id == orderItem.ProductVariantId);
                if (variant == null)
                    return Result.Failure(Error.NotFound("ProductVariant.NotFound", "One of the products not found."));

                var allocateResult = orderItem.AllocateAvailableQuantity(variant.AvailableQuantity);
                if (allocateResult.IsFailure)
                    return Result.Failure(allocateResult.Error);

                var newlyAllocatedQty = allocateResult.Value;
                if(newlyAllocatedQty > 0)
                {
                    var reservedResult = variant.ReserveStock(newlyAllocatedQty);
                    if (reservedResult.IsFailure)
                        return Result.Failure(reservedResult.Error);
                }
            }

            var result = order.Confirm();
            if (result.IsFailure)
                return result;

            await _context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
    }
}
