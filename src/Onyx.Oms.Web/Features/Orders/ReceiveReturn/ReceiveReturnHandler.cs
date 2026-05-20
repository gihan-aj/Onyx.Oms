using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Orders.ReceiveReturn
{
    public class ReceiveReturnHandler : ICommandHandler<ReceiveReturnCommand>
    {
        private readonly IApplicationDbContext _context;

        public ReceiveReturnHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result> Handle(ReceiveReturnCommand request, CancellationToken cancellationToken)
        {
            var order = await _context.Orders
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);
            if (order == null)
                return Result.Failure(Error.NotFound("Order.NotFound", "Order not found."));

            if (!string.IsNullOrWhiteSpace(request.Reason))
            {
                string statusString = request.IsReceived ? "Received Return" : "Lost in Transit";
                string regressNote = $"[{DateTimeOffset.UtcNow:g}] (UTC) System Note: {statusString}: {request.Reason}.";
                string updatedNotes = string.IsNullOrWhiteSpace(order.Notes)
                    ? regressNote
                    : order.Notes + Environment.NewLine + regressNote;
                order.UpdateNotes(updatedNotes);
            }

            var result = order.ReceiveReturn(request.IsReceived);
            if (result.IsFailure)
                return result;

            await _context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
    }
}
