using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Orders.RollbackOrderToPacked
{
    public class RollbackOrderToPackedHandler : ICommandHandler<RollbackOrderToPackedCommand>
    {
        private readonly IApplicationDbContext _context;

        public RollbackOrderToPackedHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result> Handle(RollbackOrderToPackedCommand request, CancellationToken cancellationToken)
        {
            var order = await _context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

            if (order == null) 
                return Result.Failure(Error.NotFound("Order.NotFound", "Order not found."));

            var rollbackResult = order.RevertShipment(request.Reason);
            if (rollbackResult.IsFailure)
                return Result.Failure(rollbackResult.Error);

            var variantIds = order.Items.Select(i => i.ProductVariantId).ToList();
            var variants = await _context.ProductVariants
                .Where(v => variantIds.Contains(v.Id))
                .ToListAsync(cancellationToken);

            foreach(var item in order.Items)
            {
                var variant = variants.FirstOrDefault(v => v.Id == item.ProductVariantId);
                variant?.RevertShipment(item.Quantity);
            }

            await _context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
    }

}
