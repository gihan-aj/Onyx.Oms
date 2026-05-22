using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Orders.SendOrderConfirmation
{
    public record SendOrderConfirmationCommand(Guid OrderId) : ICommand<string>;
}
