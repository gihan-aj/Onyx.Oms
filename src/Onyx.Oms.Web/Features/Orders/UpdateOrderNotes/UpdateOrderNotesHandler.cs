using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Orders.UpdateOrderNotes
{
    public class UpdateOrderNotesHandler : ICommandHandler<UpdateOrderNotesCommand>
    {
        private readonly IApplicationDbContext _context;

        public UpdateOrderNotesHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result> Handle(UpdateOrderNotesCommand request, CancellationToken cancellationToken)
        {
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

            if (order == null)
                return Result.Failure(Error.NotFound("Order.NotFound", "Order not found."));

            var result = order.UpdateNotes(request.Notes);
            if (result.IsFailure)
                return result;

            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
