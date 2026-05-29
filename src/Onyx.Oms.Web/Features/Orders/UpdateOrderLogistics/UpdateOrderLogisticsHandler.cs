using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Domain.ValueObjects;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Orders.UpdateOrderLogistics
{
    public class UpdateOrderLogisticsHandler : ICommandHandler<UpdateOrderLogisticsCommand>
    {
        private readonly IApplicationDbContext _context;

        public UpdateOrderLogisticsHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result> Handle(UpdateOrderLogisticsCommand request, CancellationToken cancellationToken)
        {
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

            if (order == null)
                return Result.Failure(Error.NotFound("Order.NotFound", "Order not found."));

            if (request.CourierId.HasValue)
            {
                bool courierExists = await _context.Couriers
                    .AnyAsync(c => c.Id == request.CourierId && c.IsActive, cancellationToken);
                if (!courierExists)
                    return Result.Failure(Error.NotFound("Courier.NotFound", "Courier is not found or inactive."));
            }

            Address address = Address.Empty;
            if (request.ShippingAddress is not null)
            {
                address = new Address(
                    request.ShippingAddress.Street ?? string.Empty,
                    request.ShippingAddress.City ?? string.Empty,
                    request.ShippingAddress.District ?? string.Empty,
                    request.ShippingAddress.State ?? string.Empty,
                    request.ShippingAddress.PostalCode ?? string.Empty,
                    request.ShippingAddress.Country ?? string.Empty);
            }

            var result = order.UpdateLogistics(request.CourierId, address, request.DeliveryInstructions);
            if (result.IsFailure)
                return result;

            if(!string.IsNullOrWhiteSpace(request.TrackingNumber))
                order.SetTrackingNumber(request.TrackingNumber);

            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
