using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Enums;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Domain.ValueObjects;

namespace Onyx.Oms.Core.Domain.Entities;

public class OrderItem : AuditableEntity<Guid>, IMustHaveTenant
{
    private OrderItem() { }

    private OrderItem(
        Guid tenantId,
        Guid orderId,
        Guid productVariantId,
        int quantity,
        int allocatedQuantity,
        Money unitPrice,
        OrderItemStatus status) : base(Guid.NewGuid())
    {
        TenantId = tenantId;
        OrderId = orderId;
        ProductVariantId = productVariantId;
        Quantity = quantity;
        UnitPrice = unitPrice;
        Status = status;
    }

    public Guid TenantId { get; private set; }
    public Guid OrderId { get; private set; }
    public Guid ProductVariantId { get; private set; }
    public int Quantity { get; private set; }
    public int AllocatedQuantity { get; private set; }
    public int PendingQuantity => Quantity - AllocatedQuantity;
    public Money UnitPrice { get; private set; } = Money.Zero();
    public Money DiscountAmount { get; private set; } = Money.Zero();
    public string? DiscountReason { get; private set; }
    public OrderItemStatus Status { get; private set; }

    public Money LineTotal {  get; private set; } = Money.Zero();

    public static Result<OrderItem> Create(
        Guid tenantId,
        Guid orderId,
        Guid productVariantId,
        int quantity,
        int allocatedQuantity,
        Money unitPrice)
    {
        if (orderId == Guid.Empty)
            return Result.Failure<OrderItem>(Error.Validation("OrderItem.OrderIdRequired", "Order ID is required."));

        if (productVariantId == Guid.Empty)
            return Result.Failure<OrderItem>(Error.Validation("OrderItem.ProductVariantIdRequired", "Product Variant ID is required."));

        if (quantity <= 0)
            return Result.Failure<OrderItem>(Error.Validation("OrderItem.QuantityInvalid", "Quantity must be greater than zero."));

        var status = allocatedQuantity >= quantity
            ? OrderItemStatus.Allocated
            : OrderItemStatus.Pending;

        var item = new OrderItem(
            tenantId,
            orderId,
            productVariantId,
            quantity,
            allocatedQuantity,
            unitPrice,
            status);

        item.CalculateLineTotal();
        return item;
    }

    public Result UpdateStatus(OrderItemStatus newStatus)
    {
        // Add specific validation business logic here, e.g., transitions
        if (Status == OrderItemStatus.Ready && newStatus != OrderItemStatus.Ready)
            return Result.Failure(Error.Validation("OrderItem.InvalidStatusTransition", "Cannot move a ready item backwards."));

        Status = newStatus;
        return Result.Success();
    }

    private void CalculateLineTotal()
    {
        decimal finalUnitPrice = Math.Max(0, UnitPrice.Amount - DiscountAmount.Amount);
        LineTotal = new Money(finalUnitPrice * Quantity, UnitPrice.Currency);
    }

    public Result ApplyDiscount(decimal discountValue, DiscountType type, string? reason = null)
    {
        var baseLineTotal = UnitPrice.Amount * Quantity;

        if(type == DiscountType.Percentage)
        {
            decimal calculated = baseLineTotal * (discountValue / 100m);
            DiscountAmount = new Money(calculated, UnitPrice.Currency);
        }
        else
        {
            DiscountAmount = new Money(Math.Min(discountValue, baseLineTotal), UnitPrice.Currency);
        }

        DiscountReason = reason;
        CalculateLineTotal();
        return Result.Success();
    }
}
