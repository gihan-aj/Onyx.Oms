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

            var taskResult = productionTask.StartWork(request.QuantityToStart);
            if (taskResult.IsFailure)
                return Result.Failure(taskResult.Error);

            await _context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
    }
}
