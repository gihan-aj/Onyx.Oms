using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Enums;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Dashboard.GetMainSummary
{
    public class GetMainDashboardSummaryHandler : IQueryHandler<GetMainDashboardSummaryQuery, MainDashboardSummaryDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public GetMainDashboardSummaryHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<Result<MainDashboardSummaryDto>> Handle(GetMainDashboardSummaryQuery request, CancellationToken cancellationToken)
        {
            var now = DateTimeOffset.UtcNow;
            var startOfDay = now.Date;

            // User Info
            string displayName = "User";
            if (_currentUserService.UserId.HasValue)
            {
                var user = await _context.AppUsers
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == _currentUserService.UserId.Value, cancellationToken);

                if (user != null)
                {
                    displayName = string.IsNullOrWhiteSpace(user.FirstName) ? user.Email! : $"{user.FirstName} {user.LastName}".Trim();
                }
            }

            // Stats
            var pendingOrdersCount = await _context.Orders
                .CountAsync(o => o.Status == OrderStatus.Pending, cancellationToken);

            var readyToPackCount = await _context.Orders
                .CountAsync(o => o.Status == OrderStatus.ReadyToPack, cancellationToken);

            var tasksCompletedUnallocatedCount = await _context.FulfillmentTasks
                .CountAsync(t => t.Status == FulfillmentTaskStatus.Ready, cancellationToken);

            var shippedTodayCount = await _context.Orders
                .CountAsync(o => o.Status == OrderStatus.Shipped &&
                                 o.LastModifiedOnUtc.HasValue &&
                                 o.LastModifiedOnUtc.Value >= startOfDay, cancellationToken);

            var twelveHoursAgo = now.AddHours(-12);
            var ninetySixHoursAgo = now.AddHours(-96);

            // Action Required Counts
            var idleReadyToPackCount = await _context.Orders
                .CountAsync(o => o.Status == OrderStatus.ReadyToPack &&
                                 o.LastModifiedOnUtc.HasValue &&
                                 o.LastModifiedOnUtc.Value < twelveHoursAgo, cancellationToken);

            var unpaidDeliveredCount = await _context.Orders
                .CountAsync(o => o.Status == OrderStatus.Delivered &&
                                 o.PaymentStatus == PaymentStatus.PartiallyPaid, cancellationToken);

            var rtoCount = await _context.Orders
                .CountAsync(o => o.Status == OrderStatus.ReturnedToSender, cancellationToken);

            var stalledProcessingCount = await _context.Orders
                .CountAsync(o => o.Status == OrderStatus.Processing &&
                                 o.LastModifiedOnUtc.HasValue &&
                                 o.LastModifiedOnUtc.Value < ninetySixHoursAgo, cancellationToken);

            var confirmedWithoutTasksCount = await _context.Orders
                .Where(o => o.Status == OrderStatus.Confirmed)
                .CountAsync(o => !o.Items.Any(oi => _context.FulfillmentTasks.Any(t => t.LinkedOrderItemId == oi.Id)), cancellationToken);

            int totalActionRequired = pendingOrdersCount + idleReadyToPackCount + unpaidDeliveredCount + rtoCount + stalledProcessingCount + confirmedWithoutTasksCount;

            var summary = new MainDashboardSummaryDto(
                User: new MainDashboardUserDto(displayName),
                ActionRequiredCount: totalActionRequired,
                Stats: new MainDashboardStatsDto(
                    PendingOrders: pendingOrdersCount,
                    ReadyToPack: readyToPackCount,
                    TasksCompletedUnallocated: tasksCompletedUnallocatedCount,
                    ShippedToday: shippedTodayCount)
            );
            return Result.Success(summary);
        }
    }
}
