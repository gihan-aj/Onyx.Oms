using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Orders.GenerateShippingLabel
{
    public class GenerateShippingLabelHandler : IQueryHandler<GenerateShippingLabelQuery, byte[]>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IShippingLabelGenerator _shippingLabelGenerator;

        public GenerateShippingLabelHandler(
            IApplicationDbContext context, 
            ICurrentUserService currentUserService, 
            IShippingLabelGenerator shippingLabelGenerator)
        {
            _context = context;
            _currentUserService = currentUserService;
            _shippingLabelGenerator = shippingLabelGenerator;
        }

        public async Task<Result<byte[]>> Handle(GenerateShippingLabelQuery request, CancellationToken cancellationToken)
        {
            var tenant = await _context.Tenants
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == _currentUserService.ActiveTenantId, cancellationToken);

            if (tenant == null)
                return Result.Failure<byte[]>(Error.NotFound("Tenant.NotFound", "Tenant profile not found."));

            var order = await _context.Orders
                .Include(o => o.Items)
                .Include(o => o.Payments)
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

            if (order == null)
                return Result.Failure<byte[]>(Error.NotFound("Order.NotFound", "Order not found."));

            var customer = await _context.Customers
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == order.CustomerId, cancellationToken);

            if (customer == null)
                return Result.Failure<byte[]>(Error.NotFound("Customer.NotFound", "Customer not found."));

            if (order.Status == Core.Domain.Enums.OrderStatus.Pending)
                return Result.Failure<byte[]>(Error.Validation("Order.Pending", "Shipping label cannot be generated for pending orders."));

            if(order.ShippingAddress.IsEmpty)
                return Result.Failure<byte[]>(Error.Validation("Order.ShippingAddressRequired", "Shipping label cannot be generated without a shipping address."));

            try
            {
                var labelBytes = _shippingLabelGenerator.Generate(order, customer, tenant);
                return labelBytes;
            }
            catch (Exception ex)
            {
                return Result.Failure<byte[]>(Error.Failure("InvoiceGenerationFailed", $"Failed to generate the shipping label: {ex.Message}"));
            }
        }
    }
}
