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
                .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

            if (order == null)
                return Result.Failure(Error.NotFound("Order.NotFound", "Order not found."));

            bool courierExists = await _context.Couriers.AnyAsync(c => c.Id == request.CourierId && c.IsActive, cancellationToken);
            if (!courierExists)
                return Result.Failure(Error.NotFound("Courier.NotFound", "Courier not found or inactive."));

            var result = order.Ship(request.CourierId, request.TrackingNumber);
            if (result.IsFailure)
                return result;

            await _context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
    }
}
