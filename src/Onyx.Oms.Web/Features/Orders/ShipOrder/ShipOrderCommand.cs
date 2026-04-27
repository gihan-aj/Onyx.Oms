using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Orders.ShipOrder
{
    public record ShipOrderCommand(Guid OrderId, Guid CourierId, string? TrackingNumber) : ICommand;
}
