using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.FullfillmentTasks.StartProduction
{
    public class StartProductionHandler : ICommandHandler<StartProductionCommand>
    {
        private readonly IApplicationDbContext _context;

        public StartProductionHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result> Handle(StartProductionCommand request, CancellationToken cancellationToken)
        {
            var productionTask = await _context.FulfillmentTasks
                .FirstOrDefaultAsync(t => t.Id == request.ProductionsTaskId, cancellationToken);
            if (productionTask is null)
                return Result.Failure(Error.NotFound("ProductionTask.NotFound", "Production task not found."));

            var variant = await _context.ProductVariants
                .FirstOrDefaultAsync(v => v.Id == productionTask.ProductVariantId && v.IsActive == true, cancellationToken);
            if (variant == null)
                return Result.Failure(Error.NotFound("ProductionTask.VarinatNotFound", "Product Variant not found."));

            var taskResult = productionTask.StartWork(request.QuantityToStart);
            if (taskResult.IsFailure)
                return Result.Failure(taskResult.Error);

            // Update incoming stock
            var variantUpdateResult = variant.AdjustIncomingStock(request.QuantityToStart);
            if (variantUpdateResult.IsFailure)
                return Result.Failure(variantUpdateResult.Error);

            // Order item status
            if (productionTask.LinkedOrderItemId.HasValue)
            {
                var orderItem = await _context.OrderItems
                    .FirstOrDefaultAsync(oi => oi.Id == productionTask.LinkedOrderItemId.Value, cancellationToken);

                if(orderItem != null)
                {
                    var order = await _context.Orders
                        .Include(o => o.Items)
                        .FirstOrDefaultAsync(o => o.Id == orderItem.OrderId, cancellationToken);

                    if(order != null)
                    {
                        order.UpdateStatus(Core.Domain.Enums.OrderStatus.Processing);
                        orderItem.UpdateStatus(Core.Domain.Enums.OrderItemStatus.InProduction);
                    }
                }
                
            }

            await _context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
    }
}
