using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Enums;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Domain.ValueObjects;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.FullfillmentTasks.IssuePurchaseOrder
{
    public class IssuePurchaseOrderHandler : ICommandHandler<IssuePurchaseOrderCommand>
    {
        private readonly IApplicationDbContext _context;
        public IssuePurchaseOrderHandler(IApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<Result> Handle(IssuePurchaseOrderCommand request, CancellationToken cancellationToken)
        {
            var procurementTask = await _context.FulfillmentTasks
                .FirstOrDefaultAsync(t => t.Id == request.ProcurementTaskId && t.Type == FulfillmentTaskType.Procurement, cancellationToken);

            if (procurementTask == null)
            {
                return Result.Failure(Error.NotFound("ProcurementTask.NotFound", "Procurement task not found."));
            }

            var result = procurementTask.IssuePurchaseOrder(
                request.IssueQuantity, 
                request.PurchaseOrderNumber, 
                new Money(request.Cost.Amount, request.Cost.Currency));

            if (result.IsFailure)
                return Result.Failure(result.Error);

            var splitTask = result.Value;
            if(splitTask != null)
            {
                _context.FulfillmentTasks.Add(splitTask);
            }

            await _context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
    }
}
