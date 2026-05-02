using Onyx.Oms.Core.Domain.Enums;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Orders.AddOrderPayment
{
    public record AddOrderPaymentRequest(
        decimal Amount,
        string Currency,
        PaymentMethod Method,
        string? Reference,
        DateTimeOffset PaymentDate) : ICommand<Guid>;
}
