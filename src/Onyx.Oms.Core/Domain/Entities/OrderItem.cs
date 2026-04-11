using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Enums;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Domain.ValueObjects;

namespace Onyx.Oms.Core.Domain.Entities;

public class OrderItem : AuditableEntity<Guid>
{
    private OrderItem() { }

    internal OrderItem(
        Guid id,
        Guid orderId,
        Guid productVariantId,
        int quantity,
        Money unitPrice,
        OrderItemStatus status) : base(id)
    {
        OrderId = orderId;
        ProductVariantId = productVariantId;
        Quantity = quantity;
        UnitPrice = unitPrice;
        Status = status;
    }

    public Guid OrderId { get; private set; }
    public Guid ProductVariantId { get; private set; }
    public int Quantity { get; private set; }
    public Money UnitPrice { get; private set; } = Money.Zero();
    public OrderItemStatus Status { get; private set; }

    public Money TotalPrice => new Money(UnitPrice.Amount * Quantity, UnitPrice.Currency);

    public static Result<OrderItem> Create(
        Guid orderId,
        Guid productVariantId,
        int quantity,
        Money unitPrice,
        OrderItemStatus status = OrderItemStatus.Allocated)
    {
        if (orderId == Guid.Empty)
            return Result.Failure<OrderItem>(Error.Validation("OrderItem.OrderIdRequired", "Order ID is required."));

        if (productVariantId == Guid.Empty)
            return Result.Failure<OrderItem>(Error.Validation("OrderItem.ProductVariantIdRequired", "Product Variant ID is required."));

        if (quantity <= 0)
            return Result.Failure<OrderItem>(Error.Validation("OrderItem.QuantityInvalid", "Quantity must be greater than zero."));

        return Result.Success(new OrderItem(
            Guid.NewGuid(),
            orderId,
            productVariantId,
            quantity,
            unitPrice,
            status));
    }

    public Result UpdateStatus(OrderItemStatus newStatus)
    {
        // Add specific validation business logic here, e.g., transitions
        if (Status == OrderItemStatus.Ready && newStatus != OrderItemStatus.Ready)
            return Result.Failure(Error.Validation("OrderItem.InvalidStatusTransition", "Cannot move a ready item backwards."));

        Status = newStatus;
        return Result.Success();
    }
}
