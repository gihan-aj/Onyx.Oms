using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Catalog.Dashboard.GetSummary
{
    public record GetCatalogDashboardSummaryQuery(int LowStockThreshold) : IQuery<CatalogDashboardSummaryDto>;

    public record CatalogDashboardSummaryDto(
        int TotalVariantCount,
        int InactiveVariantCount,
        int OutOfStockCount,
        int LowStockCount,
        InboundSummaryDto Inbound,
        StockTotalsDto StockTotals,
        FulfillmentTasksSummaryDto FulfillmentTasks);
    public record InboundSummaryDto(int VariantCount, int TotalUnits);
    public record StockTotalsDto(int StockOnHand, int ReservedStock, int AvailableStock);
    public record FulfillmentTasksSummaryDto(int InProduction, int Procurement);
}
