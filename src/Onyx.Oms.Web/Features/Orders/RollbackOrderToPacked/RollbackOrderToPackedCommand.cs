using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Orders.RollbackOrderToPacked
{
    public record RollbackOrderToPackedCommand(Guid OrderId, string Reason) : ICommand;
}
