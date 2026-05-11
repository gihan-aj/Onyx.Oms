using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Orders.FailDelivery
{
    public class FailDeliveryHandler : ICommandHandler<FailDeliveryCommand>
    {
        private readonly IApplicationDbContext _context;

        public FailDeliveryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result> Handle(FailDeliveryCommand request, CancellationToken cancellationToken)
        {
            var order = await _context.Orders
                .Include(o => o.Items)
                .Include(o => o.Payments)
                .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

            if (order == null)
                return Result.Failure(Error.NotFound("Order.NotFound", "Order not found."));

            if (request.IsReturnedToSender)
            {
                var variantIds = order.Items.Select(oi => oi.ProductVariantId).Distinct().ToList();
                var variants = await _context.ProductVariants
                    .Where(v => variantIds.Contains(v.Id))
                    .ToListAsync(cancellationToken);

                foreach(var orderItem in order.Items)
                {
                    var variant = variants.FirstOrDefault(v => v.Id == orderItem.ProductVariantId);
                    if(variant != null)
                    {
                        variant.AdjustStock(orderItem.Quantity);
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(request.Reason))
            {
                string regressNote = $"[{DateTimeOffset.UtcNow:g}] (UTC) System Note: Delivery Failed: {request.Reason}.";
                string updatedNotes = string.IsNullOrWhiteSpace(order.Notes)
                    ? regressNote
                    : order.Notes + Environment.NewLine + regressNote;

                order.UpdateNotes(updatedNotes);
            }

            var result = order.FailDelivery(request.IsReturnedToSender);
            if (result.IsFailure)
                return result;

            await _context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
    }
}
