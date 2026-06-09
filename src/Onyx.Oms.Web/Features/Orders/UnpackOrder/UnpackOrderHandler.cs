using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Orders.UnpackOrder
{
    public class UnpackOrderHandler : ICommandHandler<UnpackOrderCommand>
    {
        private readonly IApplicationDbContext _context;

        public UnpackOrderHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result> Handle(UnpackOrderCommand request, CancellationToken cancellationToken)
        {
            var order = await _context.Orders
               .Include(o => o.Items)
               .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

            if (order == null)
                return Result.Failure(Error.NotFound("Order.NotFound", "Order not found."));

            var result = order.RevertPacked(request.Reason);
            if (result.IsFailure)
                return result;

            await _context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
    }

}
