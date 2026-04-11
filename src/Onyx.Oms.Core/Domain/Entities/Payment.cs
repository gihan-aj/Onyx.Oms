using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Enums;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Domain.ValueObjects;

namespace Onyx.Oms.Core.Domain.Entities;

public class Payment : AuditableEntity<Guid>
{
    private Payment() { }

    internal Payment(
        Guid id,
        Guid orderId,
        Money amount,
        PaymentMethod method,
        string? reference,
        DateTime paymentDate,
        string? gatewayName = null,
        string? gatewayTransactionId = null,
        string? gatewayPaymentStatus = null) : base(id)
    {
        OrderId = orderId;
        Amount = amount;
        Method = method;
        Reference = reference;
        PaymentDate = paymentDate;
        GatewayName = gatewayName;
        GatewayTransactionId = gatewayTransactionId;
        GatewayPaymentStatus = gatewayPaymentStatus;
    }

    public Guid OrderId { get; private set; }
    public Money Amount { get; private set; } = Money.Zero();
    public PaymentMethod Method { get; private set; }
    public string? Reference { get; private set; }
    public DateTime PaymentDate { get; private set; }
    
    // Online Payment Gateway Fields
    public string? GatewayName { get; private set; }
    public string? GatewayTransactionId { get; private set; }
    public string? GatewayPaymentStatus { get; private set; }

    public static Result<Payment> Create(
        Guid orderId,
        Money amount,
        PaymentMethod method,
        string? reference,
        DateTime paymentDate,
        string? gatewayName = null,
        string? gatewayTransactionId = null,
        string? gatewayPaymentStatus = null)
    {
        if (orderId == Guid.Empty)
            return Result.Failure<Payment>(Error.Validation("Payment.OrderIdRequired", "Order ID is required."));

        if (amount.Amount <= 0)
            return Result.Failure<Payment>(Error.Validation("Payment.AmountInvalid", "Payment amount must be greater than zero."));

        return Result.Success(new Payment(
            Guid.NewGuid(),
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
