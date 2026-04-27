using Onyx.Oms.Core.Domain.Enums;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Orders.CreateOrderProcurementTask
{
    public record CreateOrderProcurementTaskCommand(
        Guid OrderId,
        Guid OrderItemId,
        int RequestedQuantity,
        string? Notes,
        DateTimeOffset? ExpectedCompletionDate,
        TaskPriority Priority) : ICommand<Guid>;
}
