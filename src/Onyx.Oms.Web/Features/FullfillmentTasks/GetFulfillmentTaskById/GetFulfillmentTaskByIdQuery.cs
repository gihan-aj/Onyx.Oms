using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Enums;
using Onyx.Oms.Core.Domain.ValueObjects;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.FullfillmentTasks.GetFulfillmentTaskById;

public record GetFulfillmentTaskByIdQuery(Guid Id) : IQuery<FulfillmentTaskByIdDto>;

public record FulfillmentTaskByIdDto(
    Guid Id,
    FulfillmentTaskType Type,
    Guid ProductVariantId,
    string ProductName,
    bool ProductHasVariants,
    List<VariantAttributeDto>? VariantAttributes,
    int RequestedQuantity,
    Guid? LinkedOrderItemId,
    string? OrderNumber,
    Money? Cost,
    Guid? AssignedUserId,
    string? AssignedUserFirstName,
    string? AssignedUserLastName,
    string? PurchaseOrderNumber,
    string? Notes,
    DateTimeOffset? ExpectedCompletionDate,
    TaskPriority Priority,
    FulfillmentTaskStatus Status,
    DateTimeOffset CreatedOnUtc,
    int StartedQuantity,
    int CompletedQuantity,
    int ScrappedQuantity);

public record VariantAttributeDto(
    string Name,
    string Value
);
