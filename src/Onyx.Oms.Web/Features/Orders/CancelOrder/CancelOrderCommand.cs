using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Orders.CancelOrder
{
    public record CancelOrderCommand(Guid OrderId, string? Reason) : ICommand;
}
