using Onyx.Oms.Core.Domain.Enums;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.FullfillmentTasks.CreateProcurementTask
{
    public record CreateProcurementTaskCommand(
        Guid ProductVariantId,
        int RequestedQuantity,
        MoneyDto Cost,
        string PurchaseOrderNumber,
        string? Notes,
        DateTimeOffset? ExpectedCompletionDate,
        TaskPriority Priority) : ICommand<Guid>;

    public record MoneyDto(decimal Amount, string Currency = "LKR");
}
