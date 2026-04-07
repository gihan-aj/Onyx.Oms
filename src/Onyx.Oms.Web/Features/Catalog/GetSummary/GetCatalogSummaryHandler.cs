using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Catalog.GetSummary
{
    public class GetCatalogSummaryHandler : IQueryHandler<GetCatalogSummaryQuery, CatalogSummaryDto>
    {
        private readonly IApplicationDbContext _context;

        public GetCatalogSummaryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<CatalogSummaryDto>> Handle(GetCatalogSummaryQuery request, CancellationToken cancellationToken)
        {
            int lowStockThreshold = 10;

            var totalCategories = await _context.ProductCategories
                .Where(c => c.IsActive)
                .CountAsync(cancellationToken);

            var productStats = await _context.Products
                .GroupBy(p => 1)
                .Select(g => new 
                {
                    Total = g.Count(),
                    Active = g.Count(p => p.IsActive),
                })
                .FirstOrDefaultAsync(cancellationToken);

            var variantStats = await _context.ProductVariants
                .Where(v => v.IsActive)
                .GroupBy(v => 1)
                .Select(g => new
                {
                    TotalActive = g.Count(),
                    OutOfStock = g.Count(v => (v.StockOnHand - v.ReservedQuantity) <= 0),
                    LowStock = g.Count(v => (v.StockOnHand - v.ReservedQuantity) > 0 && (v.StockOnHand - v.ReservedQuantity) <= lowStockThreshold),
                })
                .FirstOrDefaultAsync (cancellationToken);

            var safeProductStats = productStats ?? new { Total = 0, Active = 0 };
            var safeVariantStats = variantStats ?? new { TotalActive = 0, OutOfStock = 0, LowStock = 0 };

            var summary = new CatalogSummaryDto(
                TotalCategories: totalCategories,
                TotalProducts: safeProductStats.Total,
                ActiveProducts: safeProductStats.Active,
                TotalActiveVariants: safeVariantStats.TotalActive,
                OutOfStockVariants: safeVariantStats.OutOfStock,
                LowStockVariants: safeVariantStats.LowStock
            );

            return Result.Success(summary);
        }
    }
}
