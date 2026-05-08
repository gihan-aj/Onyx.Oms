using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Entities;
using Onyx.Oms.Core.Domain.Enums;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Orders.CreateOrderProcurementTask
{
    public class CreateOrderProcurementTaskHandler : ICommandHandler<CreateOrderProcurementTaskCommand, Guid>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public CreateOrderProcurementTaskHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<Result<Guid>> Handle(CreateOrderProcurementTaskCommand request, CancellationToken cancellationToken)
        {
            Guid? tenantId = _currentUserService.ActiveTenantId;
            if (tenantId == null)
                return Result.Failure<Guid>(Error.Unauthorized("User.TenantIdMissing", "Tenant Id not found."));

            var order = await _context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

            if (order == null)
                return Result.Failure<Guid>(Error.NotFound("Order.NotFound", "Order not found."));

            if (order.Status < OrderStatus.Confirmed || order.Status >= OrderStatus.Packed)
                return Result.Failure<Guid>(Error.Validation("Order.InvalidStatus", "Cannot create tasks for an order that is not Confirmed or Processing."));

            var orderItem = order.Items.FirstOrDefault(i => i.Id == request.OrderItemId);
            if (orderItem == null)
                return Result.Failure<Guid>(Error.NotFound("OrderItem.NotFound", "Order item not found in this order."));

            var taskResult = FulfillmentTask.Create(
                tenantId.Value,
                FulfillmentTaskType.Procurement,
                orderItem.ProductVariantId,
                request.RequestedQuantity,
                orderItem.Id,
                null, // Cost
                null, // AssignedUserId
                null, // PurchaseOrderNumber
                request.Notes,
                request.ExpectedCompletionDate,
                request.Priority);

            if (taskResult.IsFailure)
                return Result.Failure<Guid>(taskResult.Error);

            var updateItemResult = orderItem.UpdateStatus(OrderItemStatus.Ordered);
            if (updateItemResult.IsFailure)
                return Result.Failure<Guid>(updateItemResult.Error);

            _context.FulfillmentTasks.Add(taskResult.Value);
            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success(taskResult.Value.Id);
        }
    }
}
