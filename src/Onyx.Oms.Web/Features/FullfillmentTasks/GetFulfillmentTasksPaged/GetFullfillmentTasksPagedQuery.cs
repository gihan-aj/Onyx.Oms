using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Enums;
using Onyx.Oms.Core.Domain.ValueObjects;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.FullfillmentTasks.GetFulfillmentTasksPaged
{
    public record GetFullfillmentTasksPagedQuery() : PagedRequest, IQuery<PagedResult<FulfillmentTaskDto>>
    {
        public FulfillmentTaskType? Type { get; init; }
        public TaskPriority? Priority { get; init; }
        public DateTimeOffset? ExpectedCompletionDate { get; init; }
        public string? OrderNumber { get; init; }
    }

    public record FulfillmentTaskDto(
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
        TaskPriority Priority);

    public record VariantAttributeDto(
        string Name,
        string Value
    );
}
