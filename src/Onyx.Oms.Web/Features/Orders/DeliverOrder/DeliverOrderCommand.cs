using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Orders.DeliverOrder
{
    public record DeliverOrderCommand(Guid OrderId) : ICommand;
}
