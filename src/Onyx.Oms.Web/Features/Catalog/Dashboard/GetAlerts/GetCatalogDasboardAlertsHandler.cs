using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Catalog.Dashboard.GetAlerts
{
    public class GetCatalogDasboardAlertsHandler : IQueryHandler<GetCatalogDashboardAlertsQuery, CatalogDashboardAlertsDto>
    {
        private readonly IApplicationDbContext _context;

        public GetCatalogDasboardAlertsHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<CatalogDashboardAlertsDto>> Handle(GetCatalogDashboardAlertsQuery request, CancellationToken cancellationToken)
        {
            int safeLimit = Math.Min(request.Limit, 100);

            var outOfStock = await _context.ProductVariants
                .Include(v => v.Product)
                .Where(v => v.IsActive && (v.StockOnHand - v.ReservedQuantity) <= 0)
                .OrderBy(v => v.Product.Name)
                .Take(safeLimit)
                .ToListAsync(cancellationToken);

            var lowStock = await _context.ProductVariants
                .Include(v => v.Product)
                .Where(v => v.IsActive && (v.StockOnHand - v.ReservedQuantity) > 0 && (v.StockOnHand - v.ReservedQuantity) <= request.LowStockThreshold)
                .OrderBy(v => v.StockOnHand - v.ReservedQuantity)
                .Take(safeLimit)
                .ToListAsync(cancellationToken);

            var dto = new CatalogDashboardAlertsDto(
                OutOfStock: outOfStock.Select(v => new StockAlertItemDto(
                    ProductId: v.ProductId,
                    ProductName: v.Product.Name,
                    VariantId: v.Id,
                    VariantLabel: v.DisplayName,
                    AvailableStock: v.StockOnHand - v.ReservedQuantity
                )).ToList(),
                LowStock: lowStock.Select(v => new StockAlertItemDto(
                    ProductId: v.ProductId,
                    ProductName: v.Product.Name,
                    VariantId: v.Id,
                    VariantLabel: v.DisplayName,
                    AvailableStock: v.StockOnHand - v.ReservedQuantity
                )).ToList()
            );
            return Result.Success(dto);
        }
    }
}
