using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Orders.CompleteOrder
{
    public record CompleteOrderCommand(Guid OrderId) : ICommand;
}
