using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Dashboard.GetMainSummary
{
    public record GetMainDashboardSummaryQuery() : IQuery<MainDashboardSummaryDto>;

    public record MainDashboardSummaryDto(
        MainDashboardUserDto User,
        int ActionRequiredCount,
        MainDashboardStatsDto Stats);
    public record MainDashboardUserDto(string DisplayName);
    public record MainDashboardStatsDto(
        int PendingOrders,
        int ReadyToPack,
        int TasksCompletedUnallocated,
        int ShippedToday);
}
