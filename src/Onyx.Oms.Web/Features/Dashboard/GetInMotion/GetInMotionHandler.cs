using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Enums;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Dashboard.GetInMotion
{
    public class GetInMotionHandler : IQueryHandler<GetInMotionQuery, InMotionListDto>
    {
        private readonly IApplicationDbContext _context;

        public GetInMotionHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<InMotionListDto>> Handle(GetInMotionQuery request, CancellationToken cancellationToken)
        {
            var items = new List<InMotionItemDto>();

            // Active Tasks
            var tasks = await _context.FulfillmentTasks
                .Where(t => t.Status == FulfillmentTaskStatus.InProgress)
                .ToListAsync(cancellationToken);

            foreach ( var t in tasks )
            {
                var variant = await _context.ProductVariants.FindAsync(new object[] { t.ProductVariantId }, cancellationToken);
                string vLabel = variant?.Sku ?? "Unknown Variant";

                string? orderNumber = null;
                Guid? orderId = null;

                if (t.LinkedOrderItemId.HasValue)
                {
                    var orderItem = await _context.OrderItems.Include(oi => oi.Order).FirstOrDefaultAsync(oi => oi.Id == t.LinkedOrderItemId.Value, cancellationToken);
                    if (orderItem != null)
                    {
                        orderNumber = orderItem.Order?.OrderNumber ?? "Unknown Order";
                        orderId = orderItem.Order?.Id ?? Guid.Empty;
                    }
                }

                bool isOrphaned = !t.LinkedOrderItemId.HasValue;
                string ctxLabel = isOrphaned ? "No linked order · completes to stock" : $"In Progress · linked to #{orderNumber}";

                items.Add(new InMotionItemDto(
                    Type: "task",
                    TaskId: t.Id,
                    VariantLabel: vLabel,
                    TaskType: t.Type.ToString(),
                    TaskStatus: t.Status.ToString(),
                    Quantity: t.RequestedQuantity,
                    LinkedOrderNumber: orderNumber,
                    LinkedOrderId: orderId,
                    IsOrphaned: isOrphaned,
                    OrderId: null,
                    OrderNumber: null,
                    CustomerName: null,
                    OrderStatus: null,
                    TrackingNumber: null,
                    ContextLabel: ctxLabel
                ));
            }

            // Shipped Orders
            var shippedOrders = await _context.Orders
                .Where(o => o.Status == OrderStatus.Shipped)
                .OrderByDescending(o => o.LastModifiedOnUtc)
                .Take(request.Limit)
                .ToListAsync(cancellationToken);

            var customerIds = shippedOrders.Select(o => o.CustomerId).Distinct().ToList();
            var customers = await _context.Customers
                .Where(c => customerIds.Contains(c.Id))
                .ToDictionaryAsync(c => c.Id, c => c, cancellationToken);

            foreach (var o in shippedOrders)
            {
                items.Add(new InMotionItemDto(
                    Type: "order",
                    TaskId: null,
                    VariantLabel: null,
                    TaskType: null,
                    TaskStatus: null,
                    Quantity: null,
                    LinkedOrderNumber: null,
                    LinkedOrderId: null,
                    IsOrphaned: null,
                    OrderId: o.Id,
                    OrderNumber: o.OrderNumber,
                    CustomerName: customers.TryGetValue(o.CustomerId, out var customer) ? customer.Name : "Unknown",
                    OrderStatus: o.Status.ToString(),
                    TrackingNumber: o.TrackingNumber,
                    ContextLabel: "With courier · tracking assigned"
                ));
            }

            // Bring orphaned tasks to the end
            var sortedItems = items.OrderBy(x => x.IsOrphaned == true ? 1 : 0).ToList();
            return Result.Success(new InMotionListDto(sortedItems.Count, sortedItems.Take(request.Limit).ToList()));
        }
    }
}
