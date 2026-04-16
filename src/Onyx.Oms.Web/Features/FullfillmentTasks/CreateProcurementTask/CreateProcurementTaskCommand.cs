using Onyx.Oms.Core.Domain.Enums;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.FullfillmentTasks.CreateProcurementTask
{
    public record CreateProcurementTaskCommand(
        Guid ProductVariantId,
        int RequestedQuantity,
        string? Notes,
        DateTimeOffset? ExpectedCompletionDate,
        TaskPriority Priority) : ICommand<Guid>;
}
