using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Domain.ValueObjects;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.FullfillmentTasks.UpdateProcurementTask;

public class UpdateProcurementTaskHandler : ICommandHandler<UpdateProcurementTaskCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateProcurementTaskHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(UpdateProcurementTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await _context.FulfillmentTasks
            .FirstOrDefaultAsync(t => t.Id == request.ProcurementTaskId, cancellationToken);
        
        if (task is null)
            return Result.Failure(Error.NotFound("ProcurementTask.NotFound", "Procurement task not found."));

        var money = Money.Zero();
        if (request.Cost != null)
            money = new Money(request.Cost.Amount, request.Cost.Currency);

        var result = task.UpdateProcurementDetails(
            request.RequestedQuantity,
            request.PurchaseOrderNumber,
            money,
            request.ExpectedCompletionDate,
            request.Priority,
            request.Notes);

        if (result.IsFailure)
            return Result.Failure(result.Error);

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
