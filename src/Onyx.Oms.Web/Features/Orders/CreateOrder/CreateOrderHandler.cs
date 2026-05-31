using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Constants;
using Onyx.Oms.Core.Domain.Entities;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Domain.ValueObjects;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Orders.CreateOrder
{
    public class CreateOrderHandler : ICommandHandler<CreateOrderCommand, Guid>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IAppSequenceService _appSequenceService;

        public CreateOrderHandler(
            IApplicationDbContext context, 
            ICurrentUserService currentUserService, 
            IAppSequenceService appSequenceService)
        {
            _context = context;
            _currentUserService = currentUserService;
            _appSequenceService = appSequenceService;
        }

        public async Task<Result<Guid>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
        {
            Guid? tenantId = _currentUserService.ActiveTenantId;
            if (tenantId == null)
                return Result.Failure<Guid>(Error.Unauthorized("User.TenantIdMissing", "Tenant Id not found."));

            bool customerExists = await _context.Customers
                .AnyAsync(customer => customer.Id == request.CustomerId && customer.IsActive, cancellationToken);
            if (!customerExists)
                return Result.Failure<Guid>(Error.NotFound("Customer.NotFound", "Customer is not found or inactive."));

            if (request.CourierId.HasValue)
            {
                bool courierExists = await _context.Couriers
                    .AnyAsync(c => c.Id == request.CourierId && c.IsActive, cancellationToken);
                if(!courierExists)
                    return Result.Failure<Guid>(Error.NotFound("Courier.NotFound", "Courier is not found or inactive."));
            }

            Address address = Address.Empty;
            if(request.ShippingAddress is not null)
            {
                address = new Address(
                    request.ShippingAddress.Street ?? string.Empty,
                    request.ShippingAddress.City ?? string.Empty,
                    request.ShippingAddress.District ?? string.Empty,
                    request.ShippingAddress.State ?? string.Empty,
                    request.ShippingAddress.PostalCode ?? string.Empty,
                    request.ShippingAddress.Country ?? string.Empty);
            }

            var orderNumberResult = await _appSequenceService.GetNextNumberAsync(Prefixes.OrderNumber, cancellationToken);
            if (orderNumberResult.IsFailure)
                return Result.Failure<Guid>(orderNumberResult.Error);

            var orderNumber = orderNumberResult.Value;

            var orderResult = Order.Create(
                tenantId.Value,
                orderNumber,
                request.OrderDate,
                request.CustomerId,
                request.IsCashOnDelivery,
                request.CourierId,
                address,
                request.Notes,
                request.DeliveryInstructions);

            if (orderResult.IsFailure)
                return Result.Failure<Guid>(orderResult.Error);

            var order = orderResult.Value;

            if (request.Items.Any())
            {
                foreach(var item in request.Items)
                {
                    var variant = await _context.ProductVariants
                        .Include(v => v.Product)
                        .FirstOrDefaultAsync(v => v.Id == item.ProductVariantId && v.IsActive, cancellationToken);
                    if(variant is null)
                        return Result.Failure<Guid>(Error.NotFound(
                            "ProductVariant.NotFound", 
                            request.Items.Count == 1
                            ? "Product is not found."
                            : "One of the products is not found."));

                    // Do not allocate stock when creating an order! Order State -> Pending

                    var addOrderItemResult = order.AddItem(
                        item.ProductVariantId,
                        variant.DisplayName,
                        variant.Sku,
                        item.Quantity,
                        0,
                        variant.Weight ?? Weight.Zero(),
                        variant.Cost,
                        variant.Price,
                        item.Discount?.Value,
                        item.Discount?.Type,
                        item.Discount?.Reason);

                    if(addOrderItemResult.IsFailure)
                        return Result.Failure<Guid>(addOrderItemResult.Error);
                }
            }
            else
            {
                return Result.Failure<Guid>(Error.Validation("OrderItems.Required", "One or more order items are required to create an order."));
            }

            var shippingFee = request.ShippingFee != null 
                ? new Money(request.ShippingFee.Amount, request.ShippingFee.Currency)
                : Money.Zero();
            var taxAmount = request.TaxAmount != null
                ? new Money(request.TaxAmount.Amount, request.TaxAmount.Currency) 
                : Money.Zero();

            order.ApplyShippingAndTax(shippingFee, taxAmount);

            if(request.Discount != null)
            {
                order.ApplyOrderDiscount(request.Discount.Value, request.Discount.Type, request.Discount.Reason);
            }

            if(request.Payment != null)
            {
                var paymentConfig = await _context.PaymentMethodConfigs
                    .FirstOrDefaultAsync(p => p.Type == request.Payment.Method, cancellationToken);

                Money amount = new(request.Payment.Amount.Amount, request.Payment.Amount.Currency);
                var orderPaymentResult = order.AddPayment(
                    request.Payment.Method, 
                    amount, 
                    paymentConfig?.FeeRate ?? 0m,
                    request.Payment.Reference, 
                    request.Payment.Note,
                    request.Payment.PaymentDate);

                if(orderPaymentResult.IsFailure)
                    return Result.Failure<Guid>(orderPaymentResult.Error);
            }

            _context.Orders.Add(order);
            foreach(var item in order.Items)
                _context.OrderItems.Add(item);
            if (order.Payments.Any())
            {
                foreach(var item in order.Payments)
                    _context.OrderPayments.Add(item);
            }
            await _context.SaveChangesAsync(cancellationToken);

            return order.Id;
        }
    }
}
