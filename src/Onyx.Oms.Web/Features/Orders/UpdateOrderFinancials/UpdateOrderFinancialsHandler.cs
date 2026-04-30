using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Domain.ValueObjects;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Orders.UpdateOrderFinancials
{
    public class UpdateOrderFinancialsHandler : ICommandHandler<UpdateOrderFinancialsCommand>
    {
        private readonly IApplicationDbContext _context;

        public UpdateOrderFinancialsHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result> Handle(UpdateOrderFinancialsCommand request, CancellationToken cancellationToken)
        {
            var order = await _context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

            if (order == null)
                return Result.Failure(Error.NotFound("Order.NotFound", "Order not found."));

            var existingItems = await _context.OrderItems.Where(i => i.OrderId == order.Id).ToListAsync(cancellationToken);
            _context.OrderItems.RemoveRange(existingItems);

            var clearResult = order.ClearItems();
            if (clearResult.IsFailure)
                return clearResult;

            foreach(var item in request.Items)
            {
                var variant = await _context.ProductVariants
                    .FirstOrDefaultAsync(v => v.Id == item.ProductVariantId && v.IsActive, cancellationToken);
                
                if (variant == null)
                    return Result.Failure(Error.NotFound("ProductVariant.NotFound", "Product Variant not found."));

                var allocatedQuantity = variant.AvailableQuantity >= item.Quantity
                    ? item.Quantity
                    : variant.AvailableQuantity;

                var addResult = order.AddItem(
                    item.ProductVariantId,
                    variant.DisplayName,
                    variant.Sku,
                    item.Quantity,
                    allocatedQuantity,
                    variant.Price,
                    item.Discount?.Value,
                    item.Discount?.Type,
                    item.Discount?.Reason);

                if (addResult.IsFailure)
                    return addResult;
            }

            var shippingFee = request.ShippingFee != null 
                ? new Money(request.ShippingFee.Amount, request.ShippingFee.Currency)
                : Money.Zero();
                
            var taxAmount = request.TaxAmount != null
                ? new Money(request.TaxAmount.Amount, request.TaxAmount.Currency) 
                : Money.Zero();

            var applyTaxResult = order.ApplyShippingAndTax(shippingFee, taxAmount);
            if (applyTaxResult.IsFailure)
                return applyTaxResult;

            if(request.Discount != null)
            {
                var applyDiscountResult = order.ApplyOrderDiscount(request.Discount.Value, request.Discount.Type, request.Discount.Reason);
                if (applyDiscountResult.IsFailure)
                    return applyDiscountResult;
            }
            else
            {
                order.ApplyOrderDiscount(0, Onyx.Oms.Core.Domain.Enums.DiscountType.Percentage, null);
            }

            foreach(var item in order.Items)
            {
                _context.OrderItems.Add(item);
            }

            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
