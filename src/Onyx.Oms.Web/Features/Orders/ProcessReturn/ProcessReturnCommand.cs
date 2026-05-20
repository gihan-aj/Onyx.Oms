using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Orders.ProcessReturn
{
    public record ProcessReturnCommand(Guid OrderId, List<ReturnItemQuantity> ItemsToReturn, string? Reason) : ICommand;

    public record ReturnItemQuantity(Guid OrderItemId, int Quantity);
}
