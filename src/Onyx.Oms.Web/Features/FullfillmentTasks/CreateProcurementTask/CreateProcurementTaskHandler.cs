using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Entities;
using Onyx.Oms.Core.Domain.Enums;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.FullfillmentTasks.CreateProcurementTask
{
    public class CreateProcurementTaskHandler : ICommandHandler<CreateProcurementTaskCommand, Guid>
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly ICurrentUserService _currentUserService;

        public CreateProcurementTaskHandler(IApplicationDbContext dbContext, ICurrentUserService currentUserService)
        {
            _dbContext = dbContext;
            _currentUserService = currentUserService;
        }

        public async Task<Result<Guid>> Handle(CreateProcurementTaskCommand request, CancellationToken cancellationToken)
        {
            Guid? tenantId = _currentUserService.ActiveTenantId;
            if (tenantId == null)
                return Result.Failure<Guid>(Error.Unauthorized("Product.TenantIdMissing", "Tenant Id not found."));

            var variantExists = await _dbContext.ProductVariants
                .AnyAsync(pv => pv.Id == request.ProductVariantId && pv.IsActive, cancellationToken);
            if(!variantExists)
                return Result.Failure<Guid>(Error.NotFound("ProductVariant.NotFound", "Prodcut variant is not found."));

            var taskResult = FulfillmentTask.Create(
                tenantId.Value,
                FulfillmentTaskType.Procurement,
                request.ProductVariantId,
                request.RequestedQuantity,
                null,
                null,
                null,
                null,
                request.Notes,
                request.ExpectedCompletionDate,
                request.Priority);

            if (taskResult.IsFailure)
                return Result.Failure<Guid>(taskResult.Error);

            var task = taskResult.Value;

            _dbContext.FulfillmentTasks.Add(task);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return task.Id;
        }
    }
}
