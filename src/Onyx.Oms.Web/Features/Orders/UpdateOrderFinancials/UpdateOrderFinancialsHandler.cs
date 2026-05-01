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

            var requestItems = request.Items.ToList();
            var existingItems = order.Items.ToList();

            // Remove items not in request
            var itemsToRemove = existingItems.Where(ei => !requestItems.Any(ri => ri.Id.HasValue && ri.Id.Value == ei.Id)).ToList();
            foreach (var itemToRemove in itemsToRemove)
            {
                var removeResult = order.RemoveItem(itemToRemove.Id);
                if (removeResult.IsFailure) return removeResult;

                // Release reserved stock if there's any
                int releasingQty = removeResult.Value;
                if(releasingQty > 0)
                {
                    var variant = await _context.ProductVariants
                        .FirstOrDefaultAsync(v => v.Id == itemToRemove.ProductVariantId, cancellationToken);
                    if (variant is null)
                        return Result.Failure(Error.NotFound("ProductVariant.NotFound", "One of the product variants is not found."));

                    variant.ReleaseReservation(releasingQty);
                }
                _context.OrderItems.Remove(itemToRemove);
            }

            // Add or Update items
            foreach (var item in requestItems)
            {
                if (item.Id.HasValue && item.Id.Value != Guid.Empty)
                {
                    // Update
                    var updateResult = order.UpdateItem(item.Id.Value, item.Quantity, item.Discount?.Value, item.Discount?.Type, item.Discount?.Reason);
                    if (updateResult.IsFailure) 
                        return Result.Failure(updateResult.Error);

                    // Release reserved stock if there's any
                    int releasingQty = updateResult.Value;
                    if (releasingQty > 0)
                    {
                        var variant = await _context.ProductVariants
                            .FirstOrDefaultAsync(v => v.Id == item.ProductVariantId, cancellationToken);
                        if (variant is null)
                            return Result.Failure(Error.NotFound("ProductVariant.NotFound", "One of the product variants is not found."));

                        variant.ReleaseReservation(releasingQty);
                    }
                }
                else
                {
                    // Add
                    var variant = await _context.ProductVariants
                        .Include(v => v.Product)
                        .FirstOrDefaultAsync(v => v.Id == item.ProductVariantId && v.IsActive, cancellationToken);
                    
                    if (variant == null)
                        return Result.Failure(Error.NotFound("ProductVariant.NotFound", "Product Variant not found."));

                    // if order is already confirmed, reserve available quantity 
                    int allocatingQty = 0;
                    if(order.Status >= Core.Domain.Enums.OrderStatus.Confirmed)
                    {
                        allocatingQty = variant.AvailableQuantity >= item.Quantity
                            ? item.Quantity
                            : variant.AvailableQuantity;
                    }

                    var addResult = order.AddItem(
                        item.ProductVariantId,
                        variant.DisplayName,
                        variant.Sku,
                        item.Quantity,
                        allocatingQty,
                        variant.Price,
                        item.Discount?.Value,
                        item.Discount?.Type,
                        item.Discount?.Reason);

                    if (addResult.IsFailure)
                        return addResult;
                }
            }

            if(order.Status == Core.Domain.Enums.OrderStatus.Confirmed ||
                order.Status == Core.Domain.Enums.OrderStatus.Processing)
            {
                bool orderReady = order.Items.All(i => i.Status == Core.Domain.Enums.OrderItemStatus.Ready || i.Status == Core.Domain.Enums.OrderItemStatus.Allocated);
                if (orderReady)
                    order.UpdateStatus(Core.Domain.Enums.OrderStatus.ReadyToPack);
            }

            else if (order.Status == Core.Domain.Enums.OrderStatus.ReadyToPack ||
                order.Status == Core.Domain.Enums.OrderStatus.Packed)
            {
                bool orderReady = order.Items.All(i => i.Status == Core.Domain.Enums.OrderItemStatus.Ready || i.Status == Core.Domain.Enums.OrderItemStatus.Allocated);
                if (!orderReady)
                    order.UpdateStatus(Core.Domain.Enums.OrderStatus.Processing);
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
            //else
            //{
            //    order.ApplyOrderDiscount(0, Onyx.Oms.Core.Domain.Enums.DiscountType.Percentage, null);
            //}



            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
