using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Orders.FailDelivery
{
    public record FailDeliveryCommand(Guid OrderId, bool IsReturnedToSender) : ICommand;
}
