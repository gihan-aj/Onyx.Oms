using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Enums;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Domain.ValueObjects;

namespace Onyx.Oms.Core.Domain.Entities;

public class OrderPayment : AuditableEntity<Guid>, IMustHaveTenant
{
    private OrderPayment() { }

    private OrderPayment(
        Guid tenantId,
        Guid orderId,
        PaymentMethod method,
        Money amount,
        Money fee,
        Money net,
        string? reference,
        string? note,
        DateTimeOffset paymentDate,
        string? gatewayName = null,
        string? gatewayTransactionId = null,
        string? gatewayPaymentStatus = null) : base(Guid.NewGuid())
    {
        TenantId = tenantId;
        OrderId = orderId;
        Method = method;
        Amount = amount;
        GatewayFee = fee;
        Received = net;
        Reference = string.IsNullOrWhiteSpace(reference) ? null : reference;
        Note = string.IsNullOrWhiteSpace(note) ? null : note;
        PaymentDate = paymentDate;
        GatewayName = gatewayName;
        GatewayTransactionId = gatewayTransactionId;
        GatewayPaymentStatus = gatewayPaymentStatus;
    }

    public Guid TenantId { get; private set; }
    public Guid OrderId { get; private set; }
    public PaymentMethod Method { get; private set; }
    public Money Amount { get; private set; } = Money.Zero();
    public Money GatewayFee { get; private set; } = Money.Zero();
    public Money Received { get; private set; } = Money.Zero();
    public string? Reference { get; private set; }
    public string? Note { get; private set; }
    public DateTimeOffset PaymentDate { get; private set; }
    
    // Online Payment Gateway Fields
    public string? GatewayName { get; private set; }
    public string? GatewayTransactionId { get; private set; }
    public string? GatewayPaymentStatus { get; private set; }

    public static Result<OrderPayment> Create(
        Guid tenantId,
        Guid orderId,
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
        if (orderId == Guid.Empty)
            return Result.Failure<OrderPayment>(Error.Validation("Payment.OrderIdRequired", "Order ID is required."));

        if (amount.Amount == 0)
            return Result.Failure<OrderPayment>(Error.Validation("Payment.AmountInvalid", "Payment amount must not be zero."));

        Money fee;
        if (fixedFee != null)
            fee = fixedFee;
        else
            fee = new Money(
                Math.Round(amount.Amount * (feeRate / 100m), 2, MidpointRounding.AwayFromZero),
                amount.Currency);

        var net = amount - fee;

        return Result.Success(new OrderPayment(
            tenantId,
            orderId,
            method,
            amount,
            fee,
            net,
            reference,
            note,
            paymentDate,
            gatewayName,
            gatewayTransactionId,
            gatewayPaymentStatus));
    }

    // Just to update older records
    public void TempUpdateReceived(decimal feeRate)
    {
        var fee = new Money(
            Math.Round(Amount.Amount * feeRate, 0, MidpointRounding.AwayFromZero),
            Amount.Currency);

        GatewayFee = fee;
        Received = Amount - fee;
    }
}
