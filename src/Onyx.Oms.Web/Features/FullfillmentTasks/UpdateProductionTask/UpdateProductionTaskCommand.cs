using Onyx.Oms.Core.Domain.Enums;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.FullfillmentTasks.UpdateProductionTask;

public record UpdateProductionTaskCommand(
    Guid ProductionTaskId,
    int RequestedQuantity,
    Guid? AssignedUserId,
    DateTimeOffset? ExpectedCompletionDate,
    TaskPriority Priority,
    string? Notes) : ICommand;
