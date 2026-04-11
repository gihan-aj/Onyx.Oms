using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Enums;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Domain.ValueObjects;

namespace Onyx.Oms.Core.Domain.Entities;

public class Order : AuditableEntity<Guid>
{
    private Order() { }

    internal Order(
        Guid id,
        Guid customerId,
        Address shippingAddress,
        string? notes) : base(id)
    {
        CustomerId = customerId;
        ShippingAddress = shippingAddress;
        Notes = notes;
        
        Status = OrderStatus.Pending;
        PaymentStatus = PaymentStatus.Unpaid;
    }

    public Guid CustomerId { get; private set; }
    public OrderStatus Status { get; private set; }
    public PaymentStatus PaymentStatus { get; private set; }
    
    public Address ShippingAddress { get; private set; } = Address.Empty;
    public string? TrackingNumber { get; private set; }
    public string? Notes { get; private set; }

    private readonly List<OrderItem> _items = new();
    public virtual IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    private readonly List<Payment> _payments = new();
    public virtual IReadOnlyCollection<Payment> Payments => _payments.AsReadOnly();

    public Money TotalAmount
    {
        get
        {
            if (!_items.Any()) return Money.Zero();
            var currency = _items.First().UnitPrice.Currency;
            var total = _items.Sum(i => i.TotalPrice.Amount);
            return new Money(total, currency);
        }
    }

    public Money TotalPaid
    {
        get
        {
            if (!_payments.Any()) return TotalAmount.Amount > 0 ? Money.Zero(TotalAmount.Currency) : Money.Zero();
            var currency = _payments.First().Amount.Currency;
            var total = _payments.Sum(p => p.Amount.Amount);
            return new Money(total, currency);
        }
    }

    public Money BalanceAmount
    {
        get
        {
            var total = TotalAmount;
            var paid = TotalPaid;
            if (total.Currency != paid.Currency && paid.Amount > 0)
                throw new InvalidOperationException("Currency mismatch between total and paid amounts.");
            
            var balance = total.Amount - paid.Amount;
            return new Money(balance < 0 ? 0 : balance, total.Currency);
        }
    }

    public static Result<Order> Create(
        Guid customerId,
        Address? shippingAddress,
        string? notes)
    {
        if (customerId == Guid.Empty)
            return Result.Failure<Order>(Error.Validation("Order.CustomerRequired", "Customer is required."));

        return Result.Success(new Order(
            Guid.NewGuid(),
            customerId,
            shippingAddress ?? Address.Empty,
            notes));
    }

    public Result AddItem(Guid productVariantId, int quantity, Money unitPrice, OrderItemStatus initialStatus = OrderItemStatus.Allocated)
    {
        if (Status != OrderStatus.Pending)
            return Result.Failure(Error.Validation("Order.CannotModifyItems", "Cannot add items unless order is in Pending status."));

        var itemResult = OrderItem.Create(Id, productVariantId, quantity, unitPrice, initialStatus);
        
        if (itemResult.IsFailure)
            return Result.Failure(itemResult.Error);

        _items.Add(itemResult.Value);
        return Result.Success();
    }

    public Result AddPayment(Money amount, PaymentMethod method, string? reference, DateTime paymentDate, string? gatewayName = null, string? gatewayTransactionId = null, string? gatewayPaymentStatus = null)
    {
        var paymentResult = Payment.Create(Id, amount, method, reference, paymentDate, gatewayName, gatewayTransactionId, gatewayPaymentStatus);
        
        if (paymentResult.IsFailure)
            return Result.Failure(paymentResult.Error);

        _payments.Add(paymentResult.Value);
        
        UpdatePaymentStatus();

        return Result.Success();
    }

    private void UpdatePaymentStatus()
    {
        if (TotalPaid.Amount >= TotalAmount.Amount)
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

        bool hasCodPayment = _payments.Any(p => p.Method == PaymentMethod.CashOnDelivery);
        bool hasPaidAmount = TotalPaid.Amount > 0;

        if (!hasCodPayment && !hasPaidAmount)
            return Result.Failure(Error.Validation("Order.PaymentRequired", "Order cannot be confirmed unless marked COD or has a payment record."));
        
        UpdateStatus(OrderStatus.Confirmed);
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
