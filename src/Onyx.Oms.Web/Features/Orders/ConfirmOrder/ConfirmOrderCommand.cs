using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Orders.ConfirmOrder
{
    public record ConfirmOrderCommand(Guid OrderId) : ICommand;
}
