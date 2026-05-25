using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Settings.PaymentMethods.ActivatePaymentMethod
{
    public record ActivatePaymentMethodCommand(Guid Id) : ICommand;
}
