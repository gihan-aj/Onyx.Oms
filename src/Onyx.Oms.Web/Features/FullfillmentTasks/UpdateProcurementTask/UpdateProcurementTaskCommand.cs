using Onyx.Oms.Core.Domain.Enums;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.FullfillmentTasks.UpdateProcurementTask;

public record UpdateProcurementTaskCommand(
    Guid ProcurementTaskId,
    int RequestedQuantity,
    string? PurchaseOrderNumber,
    MoneyDto? Cost,
    DateTimeOffset? ExpectedCompletionDate,
    TaskPriority Priority,
    string? Notes) : ICommand;

public record MoneyDto(decimal Amount, string Currency = "LKR");
