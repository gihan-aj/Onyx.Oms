using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Orders.ReceiveReturn
{
    public record ReceiveReturnCommand(Guid OrderId, bool IsReceived, string? Reason) : ICommand;
}
