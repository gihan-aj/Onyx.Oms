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
        Money amount,
        PaymentMethod method,
        string? reference,
        DateTimeOffset paymentDate,
        string? gatewayName = null,
        string? gatewayTransactionId = null,
        string? gatewayPaymentStatus = null) : base(Guid.NewGuid())
    {
        TenantId = tenantId;
        OrderId = orderId;
        Amount = amount;
        Method = method;
        Reference = reference;
        PaymentDate = paymentDate;
        GatewayName = gatewayName;
        GatewayTransactionId = gatewayTransactionId;
        GatewayPaymentStatus = gatewayPaymentStatus;
    }

    public Guid TenantId { get; private set; }
    public Guid OrderId { get; private set; }
    public Money Amount { get; private set; } = Money.Zero();
    public PaymentMethod Method { get; private set; }
    public string? Reference { get; private set; }
    public DateTimeOffset PaymentDate { get; private set; }
    
    // Online Payment Gateway Fields
    public string? GatewayName { get; private set; }
    public string? GatewayTransactionId { get; private set; }
    public string? GatewayPaymentStatus { get; private set; }

    public static Result<OrderPayment> Create(
        Guid tenantId,
        Guid orderId,
        Money amount,
        PaymentMethod method,
        string? reference,
        DateTimeOffset paymentDate,
        string? gatewayName = null,
        string? gatewayTransactionId = null,
        string? gatewayPaymentStatus = null)
    {
        if (orderId == Guid.Empty)
            return Result.Failure<OrderPayment>(Error.Validation("Payment.OrderIdRequired", "Order ID is required."));

        if (amount.Amount == 0)
            return Result.Failure<OrderPayment>(Error.Validation("Payment.AmountInvalid", "Payment amount must not be zero."));

        return Result.Success(new OrderPayment(
            tenantId,
            orderId,
            amount,
            method,
            reference,
            paymentDate,
            gatewayName,
            gatewayTransactionId,
            gatewayPaymentStatus));
    }
}
