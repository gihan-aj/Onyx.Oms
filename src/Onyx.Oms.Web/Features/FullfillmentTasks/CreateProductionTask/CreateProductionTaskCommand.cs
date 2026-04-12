using Onyx.Oms.Core.Domain.Enums;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.FullfillmentTasks.CreateProductionTask
{
    public record CreateProductionTaskCommand(
        Guid ProductVariantId,
        int RequestedQuantity,
        Guid? AssignedUserId, // Assigning a tailor or worker instead of a PO number
        string? Notes,
        DateTimeOffset? ExpectedCompletionDate,
        TaskPriority Priority) : ICommand<Guid>;
}
