using Onyx.Oms.Core.Domain.Enums;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Orders.CreateOrderProductionTask
{
    public record CreateOrderProductionTaskCommand(
        Guid OrderId,
        Guid OrderItemId,
        int RequestedQuantity,
        string? Notes,
        DateTimeOffset? ExpectedCompletionDate,
        TaskPriority Priority) : ICommand<Guid>;
}
