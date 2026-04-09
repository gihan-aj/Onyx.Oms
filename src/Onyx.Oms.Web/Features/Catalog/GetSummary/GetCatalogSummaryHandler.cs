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

            var categoryStats = await _context.ProductCategories
                .Where(c => c.IsActive)
                .GroupBy(c => 1)
                .Select(g => new 
                {
                    Total = g.Count(),
                    TotalLeaf = g.Count(c => !c.SubCategories.Any())
                })
                .FirstOrDefaultAsync(cancellationToken);

            var categoriesWithoutProducts = await _context.ProductCategories
                .Where(c => c.IsActive && !_context.Products.Any(p => p.CategoryId == c.Id))
                .CountAsync(cancellationToken);

            var productStats = await _context.Products
                .GroupBy(p => 1)
                .Select(g => new 
                {
                    Total = g.Count(),
                    Active = g.Count(p => p.IsActive),
                    Inactive = g.Count(p => !p.IsActive),
                    WithoutImages = g.Count(g => !g.Images.Any())
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
                    TotalStockOnHand = g.Sum(v => v.StockOnHand),
                    TotalReservedQuantity = g.Sum(v => v.ReservedQuantity)
                })
                .FirstOrDefaultAsync (cancellationToken);

            var safeCategoryStats = categoryStats ?? new { Total = 0, TotalLeaf = 0 };
            var safeProductStats = productStats ?? new { Total = 0, Active = 0, Inactive = 0, WithoutImages = 0 };
            var safeVariantStats = variantStats ?? new { TotalActive = 0, OutOfStock = 0, LowStock = 0, TotalStockOnHand = 0, TotalReservedQuantity = 0 };

            var summary = new CatalogSummaryDto(
                TotalCategories: safeCategoryStats.Total,
                TotalLeafCategories: safeCategoryStats.TotalLeaf,
                TotalProducts: safeProductStats.Total,
                ActiveProducts: safeProductStats.Active,
                TotalActiveVariants: safeVariantStats.TotalActive,
                OutOfStockVariants: safeVariantStats.OutOfStock,
                LowStockVariants: safeVariantStats.LowStock,
                ProductsWithoutImages: safeProductStats.WithoutImages,
                CategoriesWithoutProducts: categoriesWithoutProducts,
                InactiveProducts: safeProductStats.Inactive,
                TotalStockOnHand: safeVariantStats.TotalStockOnHand,
                TotalReservedQuantity: safeVariantStats.TotalReservedQuantity
        );

            return Result.Success(summary);
        }
    }
}
