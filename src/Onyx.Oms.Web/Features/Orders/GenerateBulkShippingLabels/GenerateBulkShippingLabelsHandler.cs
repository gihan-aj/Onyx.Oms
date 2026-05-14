using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Enums;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Orders.GenerateBulkShippingLabels
{
    public class GenerateBulkShippingLabelsHandler : IQueryHandler<GenerateBulkShippingLabelsQuery, byte[]>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IShippingLabelGenerator _shippingLabelGenerator;

        public GenerateBulkShippingLabelsHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUserService,
            IShippingLabelGenerator shippingLabelGenerator)
        {
            _context = context;
            _currentUserService = currentUserService;
            _shippingLabelGenerator = shippingLabelGenerator;
        }

        public async Task<Result<byte[]>> Handle(GenerateBulkShippingLabelsQuery request, CancellationToken cancellationToken)
        {
            if (request.OrderIds == null || !request.OrderIds.Any())
                return Result.Failure<byte[]>(Error.Validation("BulkPrint.EmptyRequest", "Please select at least one order to print."));

            var tenant = await _context.Tenants
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == _currentUserService.ActiveTenantId, cancellationToken);

            if (tenant == null)
                return Result.Failure<byte[]>(Error.NotFound("Tenant.NotFound", "Tenant profile not found."));

            // Fetch Orders (Safety Filter: Only grab ones that are ReadyToPack or Packed)
            var orders = await _context.Orders
                .Include(o => o.Items)
                .Include(o => o.Payments)
                .AsNoTracking()
                .Where(o => request.OrderIds.Contains(o.Id) &&
                           (o.Status == OrderStatus.ReadyToPack || o.Status == OrderStatus.Packed))
                .ToListAsync(cancellationToken);

            if (!orders.Any())
                return Result.Failure<byte[]>(Error.Validation("BulkPrint.NotEligible", "None of the selected orders are eligible for shipping labels. Ensure they are in Ready to Pack or Packed status."));

            // 2. Extract Customer IDs
            var customerIds = orders.Select(o => o.CustomerId).Distinct().ToList();

            // 3. Fetch Customers for O(1) lookup dictionary
            var customersDict = await _context.Customers
                .AsNoTracking()
                .Where(c => customerIds.Contains(c.Id))
                .ToDictionaryAsync(c => c.Id, cancellationToken);

            // 4. Generate the massive PDF
            try
            {
                var pdfBytes = _shippingLabelGenerator.GenerateBulk(orders, customersDict, tenant);
                return Result.Success(pdfBytes);
            }
            catch (Exception ex)
            {
                return Result.Failure<byte[]>(Error.Failure("BulkPrint.Failed", $"Failed to generate bulk shipping labels: {ex.Message}"));
            }
        }
    }
}
