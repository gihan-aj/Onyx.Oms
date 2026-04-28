using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Orders.GetOrderById
{
    public class GetOrderByIdHandler : IQueryHandler<GetOrderByIdQuery, OrderDetailsDto>
    {
        private readonly IApplicationDbContext _context;

        public GetOrderByIdHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<OrderDetailsDto>> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
        {
            var order = await _context.Orders
                .Include(o => o.Items)
                .Include(o => o.Payments)
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

            if (order == null)
                return Result.Failure<OrderDetailsDto>(Error.NotFound("Order.NotFound", "Order not found."));

            var customer = await _context.Customers
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == order.CustomerId, cancellationToken);

            if (customer == null)
                return Result.Failure<OrderDetailsDto>(Error.NotFound("Customer.NotFound", "Customer not found."));

            var dto = new OrderDetailsDto(
                order.Id,
                order.OrderNumber,
                new CustomerDetailsDto(
                    customer.Id, 
                    customer.Name, 
                    customer.PrimaryPhone, 
                    customer.SecondaryPhone, 
                    customer.Email,
                    customer.Address,
                    customer.LastOrderNumber,
                    customer.Notes),
                order.CourierId,
                order.TrackingNumber,
                order.ShippingAddress?.Street ?? "",
                order.ShippingAddress?.City ?? "",
                order.ShippingAddress?.District ?? "",
                order.ShippingAddress?.State ?? "",
                order.ShippingAddress?.PostalCode ?? "",
                order.ShippingAddress?.Country ?? "",
                order.Status,
                order.PaymentStatus,
                order.IsCashOnDelivery,
                order.Notes,
                order.SubTotal.Amount,
                order.DiscountAmount.Amount,
                order.DiscountReason,
                order.ShippingCost.Amount,
                order.TaxAmount.Amount,
                order.GrandTotal.Amount,
                order.TotalPaid.Amount,
                order.BalanceAmount.Amount,
                order.OrderDate,
                order.CreatedOnUtc,
                order.Items.Select(i => new OrderItemDetailsDto(
                    i.Id,
                    i.ProductVariantId,
                    i.Quantity,
                    i.AllocatedQuantity,
                    i.PendingQuantity,
                    i.UnitPrice.Amount,
                    i.DiscountAmount.Amount,
                    i.DiscountReason,
                    i.LineTotal.Amount,
                    i.Status
                )).ToList(),
                order.Payments.Select(p => new OrderPaymentDetailsDto(
                    p.Id,
                    p.Amount.Amount,
                    p.Method,
                    p.Reference,
                    p.PaymentDate,
                    p.GatewayName,
                    p.GatewayTransactionId,
                    p.GatewayPaymentStatus
                )).ToList()
            );

            return Result.Success(dto);
        }
    }
}
