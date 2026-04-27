using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Orders.PackOrder
{
    public record PackOrderCommand(Guid OrderId) : ICommand;
}
