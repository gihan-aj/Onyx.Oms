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
                .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

            if (order == null)
                return Result.Failure(Error.NotFound("Order.NotFound", "Order not found."));

            var result = order.FailDelivery(request.IsReturnedToSender);
            if (result.IsFailure)
                return result;

            await _context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
    }
}
