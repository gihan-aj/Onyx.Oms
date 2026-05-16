using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Entities;
using Onyx.Oms.Core.Domain.Enums;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Dashboard.GetActionRequired
{
    public class GetActionRequiredHandler : IQueryHandler<GetActionRequiredQuery, ActionRequiredListDto>
    {
        private readonly IApplicationDbContext _context;

        public GetActionRequiredHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<ActionRequiredListDto>> Handle(GetActionRequiredQuery request, CancellationToken cancellationToken)
        {
            var now = DateTimeOffset.UtcNow;
            var rawItems = new List<(Order Order, string Reason, string Label)>();

            // 1. Returned to Sender
            var rtoOrders = await _context.Orders
                .Where(o => o.Status == OrderStatus.ReturnedToSender)
                .ToListAsync(cancellationToken);
            rawItems.AddRange(rtoOrders.Select(o => (o, "returned_to_sender", "Returned · Needs attention")));

            // 2. Delivered + Unpaid
            var unpaidOrders = await _context.Orders
                .Where(o => o.Status == OrderStatus.Delivered && o.PaymentStatus == PaymentStatus.PartiallyPaid)
                .ToListAsync(cancellationToken);
            rawItems.AddRange(unpaidOrders.Select(o => (o, "unpaid_balance", "Delivered · Balance unpaid")));

            // 3. Pending
            var pendingOrders = await _context.Orders
                .Where(o => o.Status == OrderStatus.Pending)
                .ToListAsync(cancellationToken);
            rawItems.AddRange(pendingOrders.Select(o => (o, "pending_confirmation", "Saved · Not yet confirmed")));

            // 4. Confirmed without tasks
            var confirmedNoTasks = await _context.Orders
                .Where(o => o.Status == OrderStatus.Confirmed &&
                            !o.Items.Any(oi => _context.FulfillmentTasks.Any(t => t.LinkedOrderItemId == oi.Id)))
                .ToListAsync(cancellationToken);
            rawItems.AddRange(confirmedNoTasks.Select(o => (o, "missing_tasks", "Confirmed · Tasks not created")));

            var twelveHoursAgo = now.AddHours(-12);
            var ninetySixHoursAgo = now.AddHours(-96);

            // 5. Ready to Pack (idle > 12h)
            var idleReady = await _context.Orders
                .Where(o => o.Status == OrderStatus.ReadyToPack && o.LastModifiedOnUtc.HasValue && o.LastModifiedOnUtc.Value < twelveHoursAgo)
                .ToListAsync(cancellationToken);
            rawItems.AddRange(idleReady.Select(o => (o, "idle_ready_to_pack", "Ready to pack · Sitting idle")));

            // 6. Processing (stalled > 24h)
            var stalledProcessing = await _context.Orders
                .Where(o => o.Status == OrderStatus.Processing && o.LastModifiedOnUtc.HasValue && o.LastModifiedOnUtc.Value < ninetySixHoursAgo)
                .ToListAsync(cancellationToken);
            rawItems.AddRange(stalledProcessing.Select(o => (o, "stalled_processing", "Processing · Stalled tasks")));

            int total = rawItems.Count;
            var limitedRawItems = rawItems.Take(request.Limit).ToList();

            // Efficiently fetch customers only for the limited items we are actually returning
            var customerIds = limitedRawItems.Select(r => r.Order.CustomerId).Distinct().ToList();
            
            var customers = await _context.Customers
                .Where(c => customerIds.Contains(c.Id))
                .ToDictionaryAsync(c => c.Id, c => c, cancellationToken);

            var items = limitedRawItems.Select(r => MapToDto(r.Order, r.Reason, r.Label, customers)).ToList();

            return Result.Success(new ActionRequiredListDto(total, items));
        }

        private ActionRequiredItemDto MapToDto(Order o, string reason, string label, Dictionary<Guid, Customer> customers)
        {
            customers.TryGetValue(o.CustomerId, out var customer);

            return new ActionRequiredItemDto(
                Type: "order",
                OrderId: o.Id,
                OrderNumber: o.OrderNumber,
                CustomerName: customer?.Name ?? "Unknown",
                TotalAmount: o.GrandTotal.Amount,
                Currency: o.GrandTotal.Currency,
                Status: o.Status.ToString(),
                Reason: reason,
                ReasonLabel: label,
                CreatedAt: o.OrderDate
            );
        }
    }
}
