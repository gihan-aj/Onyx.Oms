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
        string productName,
        string sku,
        int quantity,
        int allocatedQuantity,
        Money unitPrice,
        OrderItemStatus status) : base(Guid.NewGuid())
    {
        TenantId = tenantId;
        OrderId = orderId;
        ProductVariantId = productVariantId;
        ProductName = productName;
        Sku = sku;
        Quantity = quantity;
        UnitPrice = unitPrice;
        Status = status;
    }

    public Guid TenantId { get; private set; }
    public Guid OrderId { get; private set; }
    public Guid ProductVariantId { get; private set; }
    public string ProductName { get; private set; } = string.Empty;
    public string Sku { get; private set; } = string.Empty;
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
        string productName,
        string sku,
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

        if (allocatedQuantity < 0)
            return Result.Failure<OrderItem>(Error.Validation("OrderItem.AllocatedQuantityInvalid", "Allocated Quantity must be greater than or equal to zero."));

        if (allocatedQuantity > quantity)
            return Result.Failure<OrderItem>(Error.Validation("OrderItem.AllocatedQuantityInvalid", "Allocated Quantity must be less than or equal to needed quantity for the order."));

        var status = allocatedQuantity == quantity
            ? OrderItemStatus.Allocated
            : OrderItemStatus.Pending;

        var item = new OrderItem(
            tenantId,
            orderId,
            productVariantId,
            productName,
            sku,
            quantity,
            allocatedQuantity,
            unitPrice,
            status);

        item.CalculateLineTotal();
        return item;
    }

    public Result UpdateStatus(OrderItemStatus newStatus)
    {
        if (Status == OrderItemStatus.Ready && newStatus != OrderItemStatus.Ready)
            return Result.Failure(Error.Validation("OrderItem.InvalidStatusTransition", "Cannot move a ready item backwards."));

        Status = newStatus;
        return Result.Success();
    }

    public Result<int> UpdateQuantity(int quantity)
    {
        int releasedQuantity = 0;

        if (quantity <= 0)
            return Result.Failure<int>(Error.Validation("OrderItem.QuantityInvalid", "Quantity must be greater than zero."));

        if (AllocatedQuantity > quantity)
        {
            releasedQuantity = AllocatedQuantity - quantity;
            AllocatedQuantity = quantity;
            Status = OrderItemStatus.Ready;
        }
        //return Result.Failure<int>(Error.Validation("OrderItem.CannotReduceQuantity", "Cannot reduce quantity below the already allocated quantity."));
        else if(AllocatedQuantity == quantity)
        {
            Status = OrderItemStatus.Ready;
        }
        else
        {
            Status = OrderItemStatus.Pending;
        }


        Quantity = quantity;
        CalculateLineTotal();

        return releasedQuantity;
    }

    public Result<int> AllocateAvailableQuantity(int availableQuantity)
    {
        if (availableQuantity < 0)
            return Result.Failure<int>(Error.Validation("OrderItem.QuantityInvalid", "Quantity to allocate must be greater than zero."));

        int newlyAllocatedQty = 0;
        if (availableQuantity >= PendingQuantity)
        {
            AllocatedQuantity += PendingQuantity;
            Status = OrderItemStatus.Ready;
            newlyAllocatedQty = PendingQuantity;
        }
        else
        {
            AllocatedQuantity += availableQuantity;
            newlyAllocatedQty = availableQuantity;
        }

        return newlyAllocatedQty;
    }

    public Result AllocateFromTask(int quantity)
    {
        if (quantity <= 0)
            return Result.Failure(Error.Validation("OrderItem.QuantityInvalid", "Quantity to allocate must be greater than zero."));

        if (quantity > PendingQuantity)
            return Result.Failure(Error.Validation("OrderItem.QuantityInvalid", "Quantity to allocate must be less than or equal to pending quanity."));

        AllocatedQuantity += quantity;
        
        if (AllocatedQuantity >= Quantity)
        {
            Status = OrderItemStatus.Ready;
        }

        return Result.Success();
    }

    private void CalculateLineTotal()
    {
        decimal baseLineTotal = UnitPrice.Amount * Quantity;
        decimal discountedTotal = Math.Max(0, baseLineTotal - DiscountAmount.Amount);
        LineTotal = new Money(discountedTotal, UnitPrice.Currency);
    }

    public Result ApplyDiscount(decimal discountValue, DiscountType type, string? reason = null)
    {
        var baseLineTotal = UnitPrice.Amount * Quantity;

        if(type == DiscountType.Percentage)
        {
            decimal calculated = baseLineTotal * (discountValue / 100m);
            decimal rounded = Math.Round(calculated, 0, MidpointRounding.AwayFromZero);
            DiscountAmount = new Money(rounded, UnitPrice.Currency);
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
