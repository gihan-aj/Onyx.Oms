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

            var variantIds = order.Items.Select(i => i.ProductVariantId).Distinct().ToList();

            var stockData = await _context.ProductVariants
                .AsNoTracking()
                .Where(v => variantIds.Contains(v.Id))
                .Select(v => new
                {
                    VariantId = v.Id,
                    v.ProductId,
                    v.Attributes,
                    AvailableStock = v.StockOnHand - v.ReservedQuantity
                })
                .ToDictionaryAsync(x => x.VariantId, x => x.AvailableStock, cancellationToken);

            var variantDataList = await _context.ProductVariants
                .AsNoTracking()
                .Where(v => variantIds.Contains(v.Id))
                .Select(v => new
                {
                    VariantId = v.Id,
                    AvailableStock = v.StockOnHand - v.ReservedQuantity,
                    IncomingStock = v.IncomingStock,
                    Attributes = v.Attributes.Select(a => new { a.Name, a.Value }).ToList(),
                    Images = v.Product.Images.Select(i => new { i.Url, i.IsMain, i.OptionName, i.OptionValue }).ToList()
                })
                .ToListAsync(cancellationToken);

            var variantDataDict = variantDataList.ToDictionary(v => v.VariantId);

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
                    customer.DeliveryInstructions,
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
                order.DeliveryInstructions,
                order.Notes,
                order.SubTotal.Amount,
                order.DiscountAmount.Amount,
                order.DiscountReason,
                order.ShippingCost.Amount,
                order.TaxAmount.Amount,
                order.GrandTotal.Amount,
                order.TotalPaid.Amount,
                order.BalanceAmount.Amount,
                order.GrandTotal.Currency,
                order.OrderDate,
                order.CreatedOnUtc,
                order.Items.Select(i =>
                {
                    var variantInfo = variantDataDict.GetValueOrDefault(i.ProductVariantId);

                    var incomingStock = 0;

                    string? resolvedImageUrl = null;
                    if(variantInfo != null && variantInfo.Images.Any())
                    {
                        incomingStock = variantInfo.IncomingStock;

                        var taggedImage = variantInfo.Images.FirstOrDefault(img => 
                            !string.IsNullOrWhiteSpace(img.OptionName) &&
                            !string.IsNullOrWhiteSpace(img.OptionValue) &&
                            variantInfo.Attributes.Any(attr =>
                                attr.Name.Equals(img.OptionName, StringComparison.OrdinalIgnoreCase) &&
                                attr.Value.Equals(img.OptionValue, StringComparison.OrdinalIgnoreCase))
                            );

                        if(taggedImage != null)
                        {
                            resolvedImageUrl = taggedImage.Url;
                        }
                        else
                        {
                            resolvedImageUrl = variantInfo.Images.FirstOrDefault(img => img.IsMain)?.Url
                                ?? variantInfo.Images.FirstOrDefault()?.Url;
                        }
                    }

                    return new OrderItemDetailsDto(
                        i.Id,
                        i.ProductVariantId,
                        i.ProductName,
                        i.Sku,
                        resolvedImageUrl,
                        variantInfo?.AvailableStock ?? 0,
                        i.Quantity,
                        i.AllocatedQuantity,
                        i.PendingQuantity,
                        incomingStock,
                        i.UnitPrice.Amount,
                        i.DiscountAmount.Amount,
                        i.DiscountReason,
                        i.LineTotal.Amount,
                        i.Status
                    );
                }).ToList(),
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
