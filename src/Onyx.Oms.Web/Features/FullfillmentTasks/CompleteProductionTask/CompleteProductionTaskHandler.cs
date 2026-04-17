using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Enums;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.FullfillmentTasks.CompleteProductionTask;

public class CompleteProductionTaskHandler : ICommandHandler<CompleteProductionTaskCommand>
{
    private readonly IApplicationDbContext _context;

    public CompleteProductionTaskHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(CompleteProductionTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await _context.FulfillmentTasks
            .FirstOrDefaultAsync(t => t.Id == request.ProductionTaskId, cancellationToken);
        
        if (task is null)
            return Result.Failure(Error.NotFound("ProductionTask.NotFound", "Production task not found."));

        if (task.Type != FulfillmentTaskType.Production)
            return Result.Failure(Error.Validation("Task.InvalidType", "The specified task is not a production task."));

        var variant = await _context.ProductVariants
                .FirstOrDefaultAsync(v => v.Id == task.ProductVariantId && v.IsActive == true, cancellationToken);
        if (variant == null)
            return Result.Failure(Error.NotFound("ProductionTask.VarinatNotFound", "Product Variant not found."));

        var result = task.MarkReady(request.QuantityToComplete);
        if (result.IsFailure)
            return Result.Failure(result.Error);

        // Update stock
        var incomingStockResult = variant.AdjustIncomingStock(-request.QuantityToComplete);
        if (incomingStockResult.IsFailure)
            return Result.Failure(incomingStockResult.Error);

        var stockResult = variant.AdjustStock(request.QuantityToComplete);
        if (stockResult.IsFailure)
            return Result.Failure(stockResult.Error);

        // TODO: If there is an order id Reserved should be added

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
