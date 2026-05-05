using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Enums;
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
                .Include(o => o.Payments)
                .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

            if (order == null)
                return Result.Failure(Error.NotFound("Order.NotFound", "Order not found."));

            var requestItems = request.Items.ToList();
            var existingItems = order.Items.ToList();

            // Remove items not in request
            var itemsToRemove = existingItems.Where(ei => !requestItems.Any(ri => ri.Id.HasValue && ri.Id.Value == ei.Id)).ToList();

            var itemsToRemoveIds = itemsToRemove.Select(i => i.Id).ToList();
            if (itemsToRemoveIds.Any())
            {
                var tasksToUnlink = await _context.FulfillmentTasks
                    .Where(t => t.LinkedOrderItemId != null && itemsToRemoveIds.Contains(t.LinkedOrderItemId.Value))
                    .ToListAsync(cancellationToken);
                foreach (var task in tasksToUnlink)
                {
                    task.UnlinkOrderItem();
                }
            }

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
                    var existingItem = existingItems.FirstOrDefault(ei => ei.Id == item.Id.Value);
                    int oldQuantity = existingItem != null ? existingItem.Quantity : item.Quantity;

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

                    if(item.Quantity > oldQuantity)
                    {
                        int quantityAdded = item.Quantity - oldQuantity;

                        var variant = await _context.ProductVariants
                            .FirstOrDefaultAsync(v => v.Id == item.ProductVariantId, cancellationToken);

                        if(variant != null)
                        {
                            int allocatingQty = variant.AvailableQuantity >= quantityAdded
                                ? quantityAdded
                                : variant.AvailableQuantity;

                            if(allocatingQty > 0)
                            {
                                variant.ReserveStock(allocatingQty);
                                var orderItemToUpdate = order.Items.First(i => i.Id == item.Id.Value);
                                orderItemToUpdate.AllocateAvailableQuantity(allocatingQty);
                            }

                            int newlyPendingQty = quantityAdded - allocatingQty;
                            if(newlyPendingQty > 0)
                            {
                                var existingTask = await _context.FulfillmentTasks
                                    .FirstOrDefaultAsync(t => t.LinkedOrderItemId != item.Id.Value
                                        && t.Status != FulfillmentTaskStatus.Cancelled
                                        && t.Status != FulfillmentTaskStatus.Ready,
                                        cancellationToken);

                                if(existingTask != null)
                                {
                                    if(existingTask.Type == FulfillmentTaskType.Production)
                                    {
                                        var updateProductionTaskResult = existingTask.UpdateProductionDetails(
                                            existingTask.RequestedQuantity + newlyPendingQty,
                                            existingTask.AssignedUserId,
                                            existingTask.ExpectedCompletionDate,
                                            existingTask.Priority,
                                            existingTask.Notes
                                        );
                                        if (updateProductionTaskResult.IsFailure)
                                            return Result.Failure(updateProductionTaskResult.Error);
                                    }
                                    else if(existingTask.Type == FulfillmentTaskType.Procurement)
                                    {
                                        var newTaskResult = Core.Domain.Entities.FulfillmentTask.Create(
                                            existingTask.TenantId,
                                            existingTask.Type,
                                            existingTask.ProductVariantId,
                                            newlyPendingQty,
                                            item.Id.Value,
                                            null, // cost
                                            null, // assigned user
                                            null, // po number
                                            "Auto-created due to order item quantity increase",
                                            null, // completion date
                                            existingTask.Priority
                                        );
                                        if (newTaskResult.IsFailure)
                                            return Result.Failure(newTaskResult.Error);

                                        _context.FulfillmentTasks.Add(newTaskResult.Value);
                                    }
                                }
                            }
                        }

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
                    if(order.Status >= OrderStatus.Confirmed)
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

                    if(allocatingQty > 0)
                    {
                        var reserveStockResult = variant.ReserveStock(allocatingQty);
                        if (reserveStockResult.IsFailure)
                            return Result.Failure(reserveStockResult.Error);
                    }
                }
            }

            if(order.Status == OrderStatus.Confirmed ||
                order.Status == OrderStatus.Processing)
            {
                bool orderReady = order.Items.All(i => i.Status == OrderItemStatus.Ready || i.Status == OrderItemStatus.Allocated);
                if (orderReady)
                    order.UpdateStatus(OrderStatus.ReadyToPack);
            }

            else if (order.Status == OrderStatus.ReadyToPack ||
                order.Status == OrderStatus.Packed)
            {
                bool orderReady = order.Items.All(i => i.Status == OrderItemStatus.Ready || i.Status == OrderItemStatus.Allocated);
                if (!orderReady)
                {
                    var oldStatus = order.Status;
                    order.UpdateStatus(OrderStatus.Confirmed);

                    string regressNote = $"[{DateTimeOffset.UtcNow:g}] System Note: Order status reverted from {oldStatus} to Processing due to item modifications.";
                    string updatedNotes = string.IsNullOrWhiteSpace(order.Notes)
                        ? regressNote
                        : order.Notes + Environment.NewLine + regressNote;

                    order.UpdateNotes(updatedNotes);
                }
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
            //    order.ApplyOrderDiscount(0, Onyx.Oms.DiscountType.Percentage, null);
            //}



            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
