using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Settings.PaymentMethods.DeactivatePaymentMethod
{
    public record DeactivatePaymentMethodCommand(Guid Id) : ICommand;
}
