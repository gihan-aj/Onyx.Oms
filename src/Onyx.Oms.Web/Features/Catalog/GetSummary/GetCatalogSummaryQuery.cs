using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Catalog.GetSummary
{
    public record GetCatalogSummaryQuery() : IQuery<CatalogSummaryDto>;

    public record CatalogSummaryDto(
        int TotalCategories,
        int TotalProducts,
        int ActiveProducts,
        int TotalActiveVariants,
        int OutOfStockVariants,
        int LowStockVariants);
}
