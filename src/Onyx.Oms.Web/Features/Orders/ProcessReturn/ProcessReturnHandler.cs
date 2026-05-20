using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Orders.ProcessReturn
{
    public class ProcessReturnHandler : ICommandHandler<ProcessReturnCommand>
    {
        private readonly IApplicationDbContext _context;

        public ProcessReturnHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result> Handle(ProcessReturnCommand request, CancellationToken cancellationToken)
        {
            var order = await _context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

            if (order == null)
                return Result.Failure(Error.NotFound("Order.NotFound", "Order not found."));

            foreach(var returnItem in request.ItemsToReturn)
            {
                var orderItem = order.Items.FirstOrDefault(i => i.Id == returnItem.OrderItemId);
                if (orderItem == null)
                    return Result.Failure(Error.Validation("ProcessReturn.InvalidItem", $"Order item {returnItem.OrderItemId} not found in this order."));

                if (returnItem.Quantity > orderItem.Quantity)
                    return Result.Failure(Error.Validation("ProcessReturn.InvalidQuantity", $"Cannot return {returnItem.Quantity} items. Only {orderItem.Quantity} were ordered for item {returnItem.OrderItemId}."));
            }

            // Fetch variants to adjust stock
            var variantIds = order.Items
                .Where(oi => request.ItemsToReturn.Any(ri => ri.OrderItemId == oi.Id))
                .Select(oi => oi.ProductVariantId)
                .Distinct()
                .ToList();

            var variants = await _context.ProductVariants
                .Where(v => variantIds.Contains(v.Id))
                .ToListAsync(cancellationToken);

            // Adjust Stock
            foreach(var returnItem in request.ItemsToReturn)
            {
                if (returnItem.Quantity <= 0)
                    continue;

                var orderItem = order.Items.First(i => i.Id == returnItem.OrderItemId);
                var variant = variants.FirstOrDefault(v => v.Id == orderItem.ProductVariantId);

                if (variant != null)
                {
                    variant.AdjustStock(returnItem.Quantity);
                }
            }

            if (!string.IsNullOrWhiteSpace(request.Reason))
            {
                string regressNote = $"[{DateTimeOffset.UtcNow:g}] (UTC) System Note: Return Processed: {request.Reason}.";
                string updatedNotes = string.IsNullOrWhiteSpace(order.Notes)
                    ? regressNote
                    : order.Notes + Environment.NewLine + regressNote;
                order.UpdateNotes(updatedNotes);
            }

            var result = order.ReturnProcess();
            if (result.IsFailure)
                return result;
            await _context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
    }
}
