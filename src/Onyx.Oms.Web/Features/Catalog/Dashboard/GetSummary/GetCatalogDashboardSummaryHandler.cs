using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Enums;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Catalog.Dashboard.GetSummary
{
    public class GetCatalogDashboardSummaryHandler : IQueryHandler<GetCatalogDashboardSummaryQuery, CatalogDashboardSummaryDto>
    {
        private readonly IApplicationDbContext _context;

        public GetCatalogDashboardSummaryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<CatalogDashboardSummaryDto>> Handle(GetCatalogDashboardSummaryQuery request, CancellationToken cancellationToken)
        {
            var variantStats = await _context.ProductVariants
                .GroupBy(v => 1)
                .Select(g => new
                {
                    TotalActive = g.Count(v => v.IsActive),
                    TotalInactive = g.Count(v => !v.IsActive),
                    OutOfStock = g.Count(v => v.IsActive && (v.StockOnHand - v.ReservedQuantity) <= 0),
                    LowStock = g.Count(v => v.IsActive && (v.StockOnHand - v.ReservedQuantity) > 0 && (v.StockOnHand - v.ReservedQuantity) <= request.LowStockThreshold),
                    InboundVariantCount = g.Count(v => v.IsActive && v.IncomingStock > 0),
                    TotalInboundUnits = g.Sum(v => v.IsActive ? v.IncomingStock : 0),
                    TotalStockOnHand = g.Sum(v => v.IsActive ? v.StockOnHand : 0),
                    TotalReservedQuantity = g.Sum(v => v.IsActive ? v.ReservedQuantity : 0)
                })
                .FirstOrDefaultAsync(cancellationToken);

            var safeVariantStats = variantStats ?? new
            {
                TotalActive = 0,
                TotalInactive = 0,
                OutOfStock = 0,
                LowStock = 0,
                InboundVariantCount = 0,
                TotalInboundUnits = 0,
                TotalStockOnHand = 0,
                TotalReservedQuantity = 0
            };

            var taskStats = await _context.FulfillmentTasks
                .Where(t => t.Status == FulfillmentTaskStatus.Pending || t.Status == FulfillmentTaskStatus.InProgress)
                .GroupBy(t => 1)
                .Select(g => new
                {
                    InProduction = g.Count(t => t.Type == FulfillmentTaskType.Production),
                    Procurement = g.Count(t => t.Type == FulfillmentTaskType.Procurement)
                })
                .FirstOrDefaultAsync(cancellationToken);

            var safeTaskStats = taskStats ?? new { InProduction = 0, Procurement = 0 };

            var summary = new CatalogDashboardSummaryDto(
                TotalVariantCount: safeVariantStats.TotalActive,
                InactiveVariantCount: safeVariantStats.TotalInactive,
                OutOfStockCount: safeVariantStats.OutOfStock,
                LowStockCount: safeVariantStats.LowStock,
                Inbound: new InboundSummaryDto(
                    VariantCount: safeVariantStats.InboundVariantCount,
                    TotalUnits: safeVariantStats.TotalInboundUnits
                ),
                StockTotals: new StockTotalsDto(
                    StockOnHand: safeVariantStats.TotalStockOnHand,
                    ReservedStock: safeVariantStats.TotalReservedQuantity,
                    AvailableStock: safeVariantStats.TotalStockOnHand - safeVariantStats.TotalReservedQuantity
                ),
                FulfillmentTasks: new FulfillmentTasksSummaryDto(
                    InProduction: safeTaskStats.InProduction,
                    Procurement: safeTaskStats.Procurement
                )
            );
            return Result.Success(summary);
        }
    }
}
