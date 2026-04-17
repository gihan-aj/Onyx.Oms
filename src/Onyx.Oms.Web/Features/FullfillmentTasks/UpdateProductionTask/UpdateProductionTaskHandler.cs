using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.FullfillmentTasks.UpdateProductionTask;

public class UpdateProductionTaskHandler : ICommandHandler<UpdateProductionTaskCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateProductionTaskHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(UpdateProductionTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await _context.FulfillmentTasks
            .FirstOrDefaultAsync(t => t.Id == request.ProductionTaskId, cancellationToken);
        
        if (task is null)
            return Result.Failure(Error.NotFound("ProductionTask.NotFound", "Production task not found."));

        if (request.AssignedUserId.HasValue)
        {
            var userExists = await _context.AppUsers.AnyAsync(u => u.Id == request.AssignedUserId.Value, cancellationToken);
            if (!userExists)
                return Result.Failure(Error.NotFound("User.NotFound", "Assigned user is not found."));
        }

        var result = task.UpdateProductionDetails(
            request.RequestedQuantity,
            request.AssignedUserId,
            request.ExpectedCompletionDate,
            request.Priority,
            request.Notes);

        if (result.IsFailure)
            return Result.Failure(result.Error);

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
