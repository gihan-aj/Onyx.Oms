using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Settings.PaymentMethods.UpdatePaymentMethod
{
    public record UpdatePaymentMethodCommand(Guid Id, string DisplayName, decimal FeeRate) : ICommand;
}
