using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Enums;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Domain.ValueObjects;

namespace Onyx.Oms.Core.Domain.Entities;

public class Order : AuditableEntity<Guid>, IMustHaveTenant
{
    private Order() { }

    private Order(
        Guid tenantId,
        string orderNumber,
        DateTimeOffset? orderDate,
        Guid customerId,
        bool isCashOnDelivery,
        Guid? courierId,
        Address shippingAddress,
        string? notes) : base(Guid.NewGuid())
    {
        TenantId = tenantId;
        OrderNumber = orderNumber;  
        OrderDate = orderDate.HasValue ? orderDate.Value : null;
        CustomerId = customerId;
        IsCashOnDelivery = isCashOnDelivery;
        CourierId = courierId.HasValue ? courierId.Value : null;
        ShippingAddress = shippingAddress;
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes;
        
        Status = OrderStatus.Pending;
        PaymentStatus = PaymentStatus.Unpaid;
    }

    public Guid TenantId { get; private set; }
    public string OrderNumber { get; private set; } = string.Empty;
    public DateTimeOffset? OrderDate { get; private set; }
    public Guid CustomerId { get; private set; }
    public OrderStatus Status { get; private set; }
    public PaymentStatus PaymentStatus { get; private set; }
    public bool IsCashOnDelivery { get; private set; }
    
    public Guid? CourierId { get; private set; }
    public Address ShippingAddress { get; private set; } = Address.Empty;
    public string? TrackingNumber { get; private set; }
    public string? Notes { get; private set; }

    public Money SubTotal { get; private set; } = Money.Zero();
    public Money DiscountAmount { get; private set; } = Money.Zero();
    public string? DiscountReason { get; private set; }
    public Money ShippingCost { get; private set; } = Money.Zero();
    public Money TaxAmount { get; private set; } = Money.Zero();
    public Money GrandTotal { get; private set; } = Money.Zero();

    private readonly List<OrderItem> _items = new();
    public virtual IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    private readonly List<OrderPayment> _payments = new();
    public virtual IReadOnlyCollection<OrderPayment> Payments => _payments.AsReadOnly();

    public Money TotalPaid
    {
        get
        {
            if (!_payments.Any()) return Money.Zero(GrandTotal.Currency);
            var total = _payments.Sum(p => p.Amount.Amount);
            return new Money(total, GrandTotal.Currency);
        }
    }

    public Money BalanceAmount => new Money(Math.Max(0, GrandTotal.Amount - TotalPaid.Amount), GrandTotal.Currency);

    public static Result<Order> Create(
        Guid tenantId,
        string orderNumber,
        DateTimeOffset? orderDate,
        Guid customerId,
        bool isCashOnDelivery,
        Guid? courierId,
        Address? shippingAddress,
        string? notes)
    {
        if (customerId == Guid.Empty)
            return Result.Failure<Order>(Error.Validation("Order.CustomerRequired", "Customer is required."));

        return Result.Success(new Order(
            tenantId,
            orderNumber,
            orderDate,
            customerId,
            isCashOnDelivery,
            courierId,
            shippingAddress ?? Address.Empty,
            notes));
    }

    public Result AddItem(
        Guid productVariantId, 
        int quantity, 
        int allocatedQuantity,
        Money unitPrice, 
        decimal? itemDiscount = null,
        DiscountType? discountType = null,
        string? discountReason = null)
    {
        if (Status != OrderStatus.Pending)
            return Result.Failure(Error.Validation("Order.CannotModifyItems", "Cannot add items unless order is in Pending status."));

        var itemResult = OrderItem.Create(TenantId, Id, productVariantId, quantity, allocatedQuantity, unitPrice);
        
        if (itemResult.IsFailure)
            return Result.Failure(itemResult.Error);

        var item = itemResult.Value;
        if (itemDiscount.HasValue && discountType.HasValue)
            item.ApplyDiscount(itemDiscount.Value, discountType.Value, discountReason);

        _items.Add(itemResult.Value);
        return Result.Success();
    }

    public Result ApplyShippingAndTax(Money shippingFee, Money taxAmount)
    {
        if (Status != OrderStatus.Pending)
            return Result.Failure(Error.Validation("Order.CannotApplyTaxAndShippingFee", $"Cannot apply Tax and Shipping fee on a {Status} Order"));

        ShippingCost = shippingFee;
        TaxAmount = taxAmount;
        RecalculateTotals();
        return Result.Success();
    }

    public Result ApplyOrderDiscount(decimal discountValue, DiscountType type, string? reason = null)
    {
        if (Status != OrderStatus.Pending)
            return Result.Failure(Error.Validation("Order.CannotDiscount", "Discounts can only be applied to pending orders."));

        if (discountValue < 0)
            return Result.Failure(Error.Validation("Order.InvalidDiscount", "Discount value cannot be negative."));

        if (!_items.Any())
            return Result.Failure(Error.Validation("Order.EmptyCart", "Cannot apply a discount to an empty order."));

        var currency = SubTotal.Currency;

        if(type == DiscountType.Percentage)
        {
            if (discountValue > 100)
                return Result.Failure(Error.Validation("Order.InvalidDiscount", "Percentage discount cannot exceed 100%."));

            decimal calculatedDiscount = SubTotal.Amount * (discountValue / 100m);
            DiscountAmount = new Money(calculatedDiscount, currency);
        }
        else
        {
            decimal flatDiscount = Math.Min(discountValue, SubTotal.Amount);
            DiscountAmount = new Money(flatDiscount, currency);
        }

        DiscountReason = reason;
        
        RecalculateTotals();

        return Result.Success();
    }

    public Result ApplyItemDiscount(Guid orderItemId, decimal value, DiscountType type, string reason)
    {
        var item = _items.FirstOrDefault(i => i.Id == orderItemId);
        if (item == null) return Result.Failure(Error.NotFound("OrderItem.NotFound", "Order item is not found"));

        var result = item.ApplyDiscount(value, type, reason);
        if (result.IsSuccess)
        {
            RecalculateTotals(); // Re-sum the SubTotal because an item got cheaper!
        }
        return result;
    }

    private void RecalculateTotals()
    {
        if(!_items.Any()) return;

        var currency = _items.First().UnitPrice.Currency;

        decimal subTotal = _items.Sum(i => i.LineTotal.Amount);
        SubTotal = new Money(subTotal, currency);

        decimal grandTotal = (subTotal + ShippingCost.Amount + TaxAmount.Amount) - DiscountAmount.Amount;
        GrandTotal = new Money(grandTotal, currency);

        UpdatePaymentStatus();
    }

    public Result AddPayment(Money amount, PaymentMethod method, string? reference, DateTime paymentDate, string? gatewayName = null, string? gatewayTransactionId = null, string? gatewayPaymentStatus = null)
    {
        var paymentResult = OrderPayment.Create(TenantId, Id, amount, method, reference, paymentDate, gatewayName, gatewayTransactionId, gatewayPaymentStatus);
        
        if (paymentResult.IsFailure)
            return Result.Failure(paymentResult.Error);

        _payments.Add(paymentResult.Value);
        
        UpdatePaymentStatus();

        return Result.Success();
    }

    private void UpdatePaymentStatus()
    {
        if (TotalPaid.Amount >= GrandTotal.Amount)
        {
            PaymentStatus = PaymentStatus.FullyPaid;
        }
        else if (TotalPaid.Amount > 0)
        {
            PaymentStatus = PaymentStatus.PartiallyPaid;
        }
        else
        {
            PaymentStatus = PaymentStatus.Unpaid;
        }
    }

    public Result Confirm()
    {
        if (Status != OrderStatus.Pending && Status != OrderStatus.PaymentFailed)
            return Result.Failure(Error.Validation("Order.InvalidStatus", "Only Pending or PaymentFailed orders can be confirmed."));

        bool hasPaidAmount = TotalPaid.Amount > 0;

        if (!IsCashOnDelivery && !hasPaidAmount)
            return Result.Failure(Error.Validation("Order.PaymentRequired", "Order cannot be confirmed unless marked COD or an advance payment is recorded."));
        
        UpdateStatus(OrderStatus.Confirmed);
        return Result.Success();
    }

    public Result Complete()
    {
        // The order must be physically in the customer's hands
        if (Status != OrderStatus.Delivered)
            return Result.Failure(Error.Validation("Order.InvalidStatus", "Order must be Delivered before it can be Completed."));

        // The money must be in account
        if (PaymentStatus != PaymentStatus.FullyPaid)
            return Result.Failure(Error.Validation("Order.Unpaid", "Order cannot be Completed until it is fully paid."));

        UpdateStatus(OrderStatus.Completed);
        return Result.Success();
    }

    public Result UpdateStatus(OrderStatus newStatus)
    {
        // Business Rule: Order cannot transition to "Ready to Pack" unless all Order Items are "Ready" or "Allocated"
        if (newStatus == OrderStatus.ReadyToPack)
        {
            bool allItemsReady = _items.All(i => i.Status == OrderItemStatus.Ready || i.Status == OrderItemStatus.Allocated);
            if (!allItemsReady)
            {
                return Result.Failure(Error.Validation("Order.ItemsNotReady", "Cannot transition to 'Ready to Pack' unless all items are 'Ready' or 'Allocated'."));
            }
        }

        Status = newStatus;
        return Result.Success();
    }

    public Result SetTrackingNumber(string trackingNumber)
    {
        if (string.IsNullOrWhiteSpace(trackingNumber))
            return Result.Failure(Error.Validation("Order.TrackingNumberRequired", "Tracking number is required."));

        TrackingNumber = trackingNumber;
        return Result.Success();
    }
}
