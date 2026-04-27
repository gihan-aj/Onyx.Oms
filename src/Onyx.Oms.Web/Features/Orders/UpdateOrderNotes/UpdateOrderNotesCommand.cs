using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Orders.UpdateOrderNotes
{
    public record UpdateOrderNotesCommand(
        Guid OrderId,
        string? Notes) : ICommand;
}
