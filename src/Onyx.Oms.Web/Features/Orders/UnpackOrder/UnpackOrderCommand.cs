using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Orders.UnpackOrder
{
    public record UnpackOrderCommand(Guid OrderId, string Reason) : ICommand;

}
