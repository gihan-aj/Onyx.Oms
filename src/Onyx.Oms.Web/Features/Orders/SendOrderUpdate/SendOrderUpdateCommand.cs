using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Orders.SendOrderUpdate
{
    public record SendOrderUpdateCommand(Guid OrderId, string LogoStoragePath) : ICommand<string>;
}
