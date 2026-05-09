using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Orders.AllocateOrderItemQuantity
{
    public record AllocateOrderItemQuantityCommand(Guid OrderId, Guid OrderItemId, int QuantityToAllocate) : ICommand;
}
