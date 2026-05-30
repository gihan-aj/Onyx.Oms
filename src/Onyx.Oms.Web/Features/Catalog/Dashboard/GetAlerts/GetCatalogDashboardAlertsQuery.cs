using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Catalog.Dashboard.GetAlerts
{
    public record GetCatalogDashboardAlertsQuery(int LowStockThreshold, int Limit = 3) : IQuery<CatalogDashboardAlertsDto>;

    public record CatalogDashboardAlertsDto(
        List<StockAlertItemDto> OutOfStock,
        List<StockAlertItemDto> LowStock);
    public record StockAlertItemDto(
        Guid ProductId,
        string ProductName,
        Guid VariantId,
        string VariantLabel,
        int AvailableStock,
        DateTimeOffset? OutOfStockSinceUtc);
}
