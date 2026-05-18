using Onyx.Oms.Core.Domain.Enums;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Orders.AddOrderPayment
{
    public record AddOrderPaymentCommand(
        Guid OrderId,
        decimal Amount,
        string Currency,
        PaymentMethod Method,
        string? Reference,
        string? Note,
        DateTimeOffset PaymentDate) : ICommand<Guid>;
}
