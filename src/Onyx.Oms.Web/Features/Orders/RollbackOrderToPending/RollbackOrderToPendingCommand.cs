using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Orders.RollbackOrderToPending
{
    public record RollbackOrderToPendingCommand(Guid OrderId, string Reason) : ICommand;

}
