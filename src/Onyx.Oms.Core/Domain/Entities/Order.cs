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
        string? notes,
        string? deliveryInstructions) : base(Guid.NewGuid())
    {
        TenantId = tenantId;
        OrderNumber = orderNumber;  
        OrderDate = orderDate.HasValue ? orderDate.Value : null;
        CustomerId = customerId;
        IsCashOnDelivery = isCashOnDelivery;
        CourierId = courierId.HasValue ? courierId.Value : null;
        ShippingAddress = shippingAddress;
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes;
        DeliveryInstructions = string.IsNullOrWhiteSpace(deliveryInstructions) ? null : deliveryInstructions;

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
    public string? DeliveryInstructions { get; private set; }

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
        string? notes,
        string? deliveryInstructions)
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
            notes,
            deliveryInstructions));
    }

    public Result<OrderItem> AddItem(
        Guid productVariantId, 
        string productName,
        string sku,
        int quantity, 
        int allocatedQuantity,
        Weight unitWeight,
        Money unitPrice, 
        decimal? itemDiscount = null,
        DiscountType? discountType = null,
        string? discountReason = null)
    {
        if (Status >= OrderStatus.Shipped)
            return Result.Failure<OrderItem>(Error.Validation("Order.CannotModifyItems", $"Cannot add items at this stage of the order (Status : {Status.ToString()})."));
        //if (Status != OrderStatus.Pending)
        //    return Result.Failure(Error.Validation("Order.CannotModifyItems", "Cannot add items unless order is in Pending status."));

        var itemResult = OrderItem.Create(TenantId, Id, productVariantId, productName, sku, quantity, allocatedQuantity, unitPrice, unitWeight);
        
        if (itemResult.IsFailure)
            return Result.Failure<OrderItem>(itemResult.Error);

        var item = itemResult.Value;
        if (itemDiscount.HasValue && discountType.HasValue)
            item.ApplyDiscount(itemDiscount.Value, discountType.Value, discountReason);

        _items.Add(itemResult.Value);
        return itemResult.Value;
    }

    public Result<int> UpdateItem(Guid orderItemId, int quantity, decimal? itemDiscount = null, DiscountType? discountType = null, string? discountReason = null)
    {
        if (Status >= OrderStatus.Shipped)
            return Result.Failure<int>(Error.Validation("Order.CannotModifyItems", $"Cannot modify items at this stage of the order (Status : {Status.ToString()})."));
        //if (Status != OrderStatus.Pending)
        //    return Result.Failure(Error.Validation("Order.CannotModifyItems", "Cannot modify items unless order is in Pending status."));

        var item = _items.FirstOrDefault(i => i.Id == orderItemId);
        if (item == null) return Result.Failure<int>(Error.NotFound("OrderItem.NotFound", "Order item not found."));

        var result = item.UpdateQuantity(quantity);
        if (result.IsFailure) return result;

        if (itemDiscount.HasValue && discountType.HasValue)
        {
            item.ApplyDiscount(itemDiscount.Value, discountType.Value, discountReason);
        }
        else
        {
            item.ApplyDiscount(0, DiscountType.Percentage, null);
        }

        RecalculateTotals();
        return result.Value;
    }

    public Result<int> RemoveItem(Guid orderItemId)
    {
        int releasingQuantity = 0;

        if (Status >= OrderStatus.Shipped)
            return Result.Failure<int>(Error.Validation("Order.CannotModifyItems", "Cannot modify items when order is shipped."));

        var item = _items.FirstOrDefault(i => i.Id == orderItemId);
        if (item == null) return Result.Failure<int>(Error.NotFound("OrderItem.NotFound", "Order item not found."));
        releasingQuantity = item.AllocatedQuantity;

        _items.Remove(item);
        RecalculateTotals();

        return releasingQuantity;
    }

    public Result ApplyShippingAndTax(Money shippingFee, Money taxAmount)
    {
        if (Status >= OrderStatus.Shipped)
            return Result.Failure(Error.Validation("Order.CannotApplyTaxAndShippingFee", $"Cannot apply Tax and Shipping fee on a {Status} Order"));
        //if (Status != OrderStatus.Pending)
        //    return Result.Failure(Error.Validation("Order.CannotApplyTaxAndShippingFee", $"Cannot apply Tax and Shipping fee on a {Status} Order"));

        ShippingCost = shippingFee;
        TaxAmount = taxAmount;
        RecalculateTotals();
        return Result.Success();
    }

    public Result ApplyOrderDiscount(decimal discountValue, DiscountType type, string? reason = null)
    {
        if (Status >= OrderStatus.Shipped)
            return Result.Failure(Error.Validation("Order.CannotDiscount", $"Discounts cannot be applied to orders at this stage (Status : {Status.ToString()})."));
        //if (Status != OrderStatus.Pending)
        //    return Result.Failure(Error.Validation("Order.CannotDiscount", "Discounts can only be applied to pending orders."));

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
            decimal roundedValue = Math.Round(calculatedDiscount, 0, MidpointRounding.AwayFromZero);
            DiscountAmount = new Money(roundedValue, currency);
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

    //public Result ClearItems()
    //{
    //    if (Status != OrderStatus.Pending)
    //        return Result.Failure(Error.Validation("Order.CannotModifyItems", "Cannot modify items unless order is in Pending status."));

    //    _items.Clear();
    //    RecalculateTotals();
    //    return Result.Success();
    //}

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

    public Result<OrderPayment> AddPayment(
        PaymentMethod method, 
        Money amount, 
        decimal feeRate, 
        string? reference, 
        string? note, 
        DateTimeOffset paymentDate,
        Money? fixedFee = null,
        string? gatewayName = null, 
        string? gatewayTransactionId = null, 
        string? gatewayPaymentStatus = null)
    {
        if (_payments.Any(p => p.Method == PaymentMethod.CashOnDelivery) && method == PaymentMethod.CashOnDelivery)
        {
            return Result.Failure<OrderPayment>(Error.Conflict("Order.InvalidPayment", "Cannot add another Cash on Delivery payment when a Cash on Delivery payment already exists."));
        }

        var paymentResult = OrderPayment.Create(
            TenantId, 
            Id, 
            method, 
            amount, 
            feeRate, 
            reference, 
            note, 
            paymentDate, 
            fixedFee,
            gatewayName, 
            gatewayTransactionId, 
            gatewayPaymentStatus);
        
        if (paymentResult.IsFailure)
            return Result.Failure<OrderPayment>(paymentResult.Error);

        _payments.Add(paymentResult.Value);
        
        UpdatePaymentStatus();

        return paymentResult.Value;
    }

    private void UpdatePaymentStatus()
    {
        var receivable = IsCashOnDelivery
            ? GrandTotal - ShippingCost
            : GrandTotal;

        if (TotalPaid.Amount >= receivable.Amount)
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

        bool areItemsReady = Items.All(i => i.PendingQuantity == 0 && (i.Status == OrderItemStatus.Allocated || i.Status == OrderItemStatus.Ready));
        if (areItemsReady)
            Status = OrderStatus.ReadyToPack;
        else
            Status = OrderStatus.Confirmed;

        return Result.Success();
    }

    public Result Complete()
    {
        // The order must be physically in the customer's hands
        //if (Status != OrderStatus.Delivered)
        //    return Result.Failure(Error.Validation("Order.InvalidStatus", "Order must be Delivered before it can be Completed."));
        if(CourierId == null)
            return Result.Failure(Error.Validation("Order.CourierRequired", "A courier is requierd before deliver and complete an order."));

        if (!ShippingAddress.IsValid)
            return Result.Failure(Error.Validation("Order.AddressRequired", "Shipping address is requierd before deliver and complete an order."));

        bool areItemsReady = Items.All(i => i.PendingQuantity == 0 && (i.Status == OrderItemStatus.Allocated || i.Status == OrderItemStatus.Ready));
        if (!areItemsReady)
            return Result.Failure(Error.Validation("Order.ItemsNotReady", "Items should be ready before deliver and complete an order."));

        // The money must be in account
        if (PaymentStatus != PaymentStatus.FullyPaid)
            return Result.Failure(Error.Validation("Order.Unpaid", "Order cannot be Completed until it is fully paid."));

        Status = OrderStatus.Completed;

        return Result.Success();
    }

    public Result<bool> MarkIfReady()
    {
        bool areAllItemsReady = false;

        areAllItemsReady = Items.All(i => i.Status == OrderItemStatus.Ready || i.Status == OrderItemStatus.Allocated);
        if(areAllItemsReady && Status < OrderStatus.ReadyToPack)
            Status = OrderStatus.ReadyToPack;

        return areAllItemsReady;
    }

    public Result Pack()
    {
        //if (Status != OrderStatus.ReadyToPack)
        //    return Result.Failure(Error.Validation("Order.InvalidStatus", "Only ReadyToPack orders can be packed."));

        bool areItemsReady = Items.All(i => i.PendingQuantity == 0 && (i.Status == OrderItemStatus.Allocated || i.Status == OrderItemStatus.Ready));
        if(!areItemsReady)
            return Result.Failure(Error.Validation("Order.ItemsNotReady", "Items should be ready before pack."));

        Status = OrderStatus.Packed;

        return Result.Success();
    }

    public Result Ship(Guid courierId, string? trackingNumber)
    {
        //if (Status != OrderStatus.Packed && Status != OrderStatus.ReadyToPack)
        //    return Result.Failure(Error.Validation("Order.InvalidStatus", "Only Packed or ReadyToPack orders can be shipped."));

        CourierId = courierId;
        if (!string.IsNullOrWhiteSpace(trackingNumber))
        {
            TrackingNumber = trackingNumber;
        }

        if(!ShippingAddress.IsValid)
            return Result.Failure(Error.Validation("Order.AddressRequired", "Shipping address is requierd before ship."));

        bool areItemsReady = Items.All(i => i.PendingQuantity == 0 && (i.Status == OrderItemStatus.Allocated || i.Status == OrderItemStatus.Ready));
        if (!areItemsReady)
            return Result.Failure(Error.Validation("Order.ItemsNotReady", "Items should be ready before ship."));

        Status = OrderStatus.Shipped;

        return Result.Success();
    }

    public Result Deliver()
    {
        //if (Status != OrderStatus.Shipped)
        //    return Result.Failure(Error.Validation("Order.InvalidStatus", "Only Shipped orders can be delivered."));
        if (CourierId == null)
            return Result.Failure(Error.Validation("Order.CourierRequired", "A courier is requierd before deliver and complete an order."));

        if (!ShippingAddress.IsValid)
            return Result.Failure(Error.Validation("Order.AddressRequired", "Shipping address is requierd before ship and deliver."));

        bool areItemsReady = Items.All(i => i.PendingQuantity == 0 && (i.Status == OrderItemStatus.Allocated || i.Status == OrderItemStatus.Ready));
        if (!areItemsReady)
            return Result.Failure(Error.Validation("Order.ItemsNotReady", "Items should be ready before ship and deliver."));

        Status = OrderStatus.Delivered;

        return Result.Success();
    }

    public Result Cancel()
    {
        if (Status >= OrderStatus.Shipped)
            return Result.Failure(Error.Validation("Order.InvalidStatus", "Cannot cancel an order that has already shipped or is further along."));

        Status = OrderStatus.Cancelled;

        //RaiseDomainEvent(new Onyx.Oms.Core.Domain.Events.OrderCancelledEvent(Id));
        return Result.Success();
    }

    public Result FailDelivery(bool isReturning)
    {
        if (Status != OrderStatus.Shipped)
            return Result.Failure(Error.Validation("Order.InvalidStatus", "Only Shipped orders can fail delivery."));

        if (isReturning)
            Status = OrderStatus.ReturnInTransit;
        else 
            Status = OrderStatus.DeliveryFailed;

        return Result.Success();
    }

    public Result ReceiveReturn(bool isReceived)
    {
        if (Status != OrderStatus.Shipped && Status != OrderStatus.ReturnInTransit)
            return Result.Failure(Error.Validation("Order.InvalidStatus", "Only Shipped orders can be received back."));

        if (isReceived)
            Status = OrderStatus.ReturnedToSender;
        else
            Status = OrderStatus.LostInTransit;
        return Result.Success();
    }

    public Result ReturnProcess()
    {
        if (Status != OrderStatus.ReturnedToSender)
            return Result.Failure(Error.Validation("Order.InvalidStatus", "Only Returned orders can be processed take back to inventory."));

        Status = OrderStatus.ReturnProcessed;
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

    public Result UpdateLogistics(Guid? courierId, Address shippingAddress, string? deliveryInstructions)
    {
        if (Status >= OrderStatus.Shipped)
            return Result.Failure(Error.Validation("Order.LogisticsLocked", "Cannot update logistics after the order has been shipped."));

        CourierId = courierId;
        ShippingAddress = shippingAddress;
        DeliveryInstructions = string.IsNullOrWhiteSpace(deliveryInstructions) ? null : deliveryInstructions;
        return Result.Success();
    }

    public Result UpdateNotes(string? notes)
    {
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes;
        return Result.Success();
    }
}
