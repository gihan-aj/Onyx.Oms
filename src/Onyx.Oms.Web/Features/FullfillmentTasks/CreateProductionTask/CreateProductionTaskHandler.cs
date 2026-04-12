using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Entities;
using Onyx.Oms.Core.Domain.Enums;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.FullfillmentTasks.CreateProductionTask
{
    public class CreateProductionTaskHandler : ICommandHandler<CreateProductionTaskCommand, Guid>
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly ICurrentUserService _currentUserService;

        public CreateProductionTaskHandler(IApplicationDbContext dbContext, ICurrentUserService currentUserService)
        {
            _dbContext = dbContext;
            _currentUserService = currentUserService;
        }

        public async Task<Result<Guid>> Handle(CreateProductionTaskCommand request, CancellationToken cancellationToken)
        {
            Guid? tenantId = _currentUserService.ActiveTenantId;
            if (tenantId == null)
                return Result.Failure<Guid>(Error.Unauthorized("FulfillmentTask.TenantIdMissing", "Tenant Id not found."));

            var variantExists = await _dbContext.ProductVariants
                .AnyAsync(pv => pv.Id == request.ProductVariantId && pv.IsActive, cancellationToken);

            if (!variantExists)
                return Result.Failure<Guid>(Error.NotFound("ProductVariant.NotFound", "Product variant is not found."));

            // Optional: If you want to validate the AssignedUserId exists before creating the task
            if (request.AssignedUserId.HasValue)
            {
                var userExists = await _dbContext.AppUsers
                    .AnyAsync(u => u.Id == request.AssignedUserId.Value, cancellationToken);

                if (!userExists)
                    return Result.Failure<Guid>(Error.NotFound("User.NotFound", "Assigned user is not found."));
            }

            var taskResult = FulfillmentTask.Create(
                tenantId: tenantId.Value,
                type: FulfillmentTaskType.Production,
                productVariantId: request.ProductVariantId,
                requestedQuantity: request.RequestedQuantity,
                linkedOrderItemId: null,
                cost: null, // Production tasks don't have an upfront PO cost in this flow
                assignedUserId: request.AssignedUserId,
                purchaseOrderNumber: null, // Not applicable for internal production
                notes: request.Notes,
                expectedCompletionDate: request.ExpectedCompletionDate,
                taskPriority: request.Priority);

            if (taskResult.IsFailure)
                return Result.Failure<Guid>(taskResult.Error);

            var task = taskResult.Value;

            _dbContext.FulfillmentTasks.Add(task);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return task.Id;
        }
    }
}
