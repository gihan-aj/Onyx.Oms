using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Enums;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Domain.ValueObjects;

namespace Onyx.Oms.Core.Domain.Entities;

public class FulfillmentTask : AuditableEntity<Guid>, IMustHaveTenant
{
    private FulfillmentTask() { }

    private FulfillmentTask(
        Guid tenantId,
        FulfillmentTaskType type,
        Guid productVariantId,
        int requestedQuantity,
        Guid? linkedOrderItemId,
        Money? cost,
        Guid? assignedUserId,
        string? purchaseOrderNumber,
        string? notes,
        DateTimeOffset? expectedCompletionDate,
        TaskPriority taskPriority) : base(Guid.NewGuid())
    {
        TenantId = tenantId;
        Type = type;
        Priority = taskPriority;
        ProductVariantId = productVariantId;
        RequestedQuantity = requestedQuantity;
        LinkedOrderItemId = linkedOrderItemId;
        Cost = cost ?? Money.Zero();
        AssignedUserId = assignedUserId;
        PurchaseOrderNumber = purchaseOrderNumber;
        Notes = notes;
        ExpectedCompletionDate = expectedCompletionDate;

        Status = FulfillmentTaskStatus.Pending;
        StartedQuantity = 0;
        CompletedQuantity = 0;
        ScrappedQuantity = 0;
    }

    public Guid TenantId { get; private set; }
    public FulfillmentTaskType Type { get; private set; }
    public FulfillmentTaskStatus Status { get; private set; }
    public TaskPriority Priority { get; private set; }
    
    public Guid ProductVariantId { get; private set; }
    public int RequestedQuantity { get; private set; }
    public int StartedQuantity { get; private set; }
    public int CompletedQuantity { get; private set; }
    public int ScrappedQuantity { get; private set; }
    
    public Money Cost { get; private set; } = Money.Zero();
    public Guid? AssignedUserId { get; private set; }
    public string? PurchaseOrderNumber { get; private set; }
    public Guid? LinkedOrderItemId { get; private set; }
    public string? Notes { get; private set; }
    public DateTimeOffset? ExpectedCompletionDate { get; private set; }

    public static Result<FulfillmentTask> Create(
        Guid tenantId,
        FulfillmentTaskType type,
        Guid productVariantId,
        int requestedQuantity,
        Guid? linkedOrderItemId = null,
        Money? cost = null,
        Guid? assignedUserId = null,
        string? purchaseOrderNumber = null,
        string? notes = null,
        DateTimeOffset? expectedCompletionDate = null,
        TaskPriority taskPriority = TaskPriority.Normal)
    {
        if (productVariantId == Guid.Empty)
            return Result.Failure<FulfillmentTask>(Error.Validation("FulfillmentTask.ProductVariantIdRequired", "Product Variant ID is required."));

        if (requestedQuantity <= 0)
            return Result.Failure<FulfillmentTask>(Error.Validation("FulfillmentTask.QuantityInvalid", "Requested quantity must be greater than zero."));

        return Result.Success(new FulfillmentTask(
            tenantId,
            type,
            productVariantId,
            requestedQuantity,
            linkedOrderItemId,
            cost,
            assignedUserId,
            purchaseOrderNumber,
            notes,
            expectedCompletionDate,
            taskPriority));
    }

    public Result StartWork(int quantityToStart)
    {
        if (Status == FulfillmentTaskStatus.Cancelled || Status == FulfillmentTaskStatus.Ready)
            return Result.Failure(Error.Validation("FulfillmentTask.InvalidStatus", "Cannot start work on a task that is completed or cancelled."));

        if (quantityToStart <= 0)
            return Result.Failure(Error.Validation("FulfillmentTask.QuantityInvalid", "Start quantity must be greater than zero."));

        if (StartedQuantity + quantityToStart > RequestedQuantity)
            return Result.Failure(Error.Validation("FulfillmentTask.ExceedsRequested", "Cannot start more items than requested."));

        StartedQuantity += quantityToStart;

        if (Status == FulfillmentTaskStatus.Pending)
        {
            Status = FulfillmentTaskStatus.InProgress;
        }

        return Result.Success();
    }

    public Result<FulfillmentTask?> IssuePurchaseOrder(int issueQuantity, string poNumber, Money cost)
    {
        if (Type != FulfillmentTaskType.Procurement)
            return Result.Failure<FulfillmentTask?>(Error.Validation("Task.InvalidType", "Only procurement tasks can be issued a PO."));

        if (Status != FulfillmentTaskStatus.Pending)
            return Result.Failure<FulfillmentTask?>(Error.Validation("Task.InvalidStatus", "Can only issue a PO for a pending task."));

        if (issueQuantity <= 0 || issueQuantity > RequestedQuantity)
            return Result.Failure<FulfillmentTask?>(Error.Validation("Task.InvalidQuantity", "Issue quantity must be greater than zero and cannot exceed requested quantity."));

        StartedQuantity = issueQuantity;

        Status = FulfillmentTaskStatus.InProgress;
        PurchaseOrderNumber = poNumber;
        Cost = cost;

        // Handle the Split if it's a partial order
        if (issueQuantity < RequestedQuantity)
        {
            int remainingQuantity = RequestedQuantity - issueQuantity;

            // Shrink current task to match what was actually ordered
            RequestedQuantity = issueQuantity;
            StartedQuantity = issueQuantity;

            // Clone a new Pending task for the leftovers
            var remainderTask = new FulfillmentTask(
                this.TenantId,
                this.Type,
                this.ProductVariantId,
                remainingQuantity, // The leftover amount
                this.LinkedOrderItemId,
                null, // No cost yet
                null, // No user assigned yet
                null, // No PO yet
                this.Notes, // Carry over notes
                this.ExpectedCompletionDate,
                this.Priority
            );

            return Result.Success<FulfillmentTask?>(remainderTask);
        }

        // Full order, no split required
        StartedQuantity = issueQuantity;
        return Result.Success<FulfillmentTask?>(null);
    }

    public Result MarkReady(int quantityToComplete)
    {
        if(Status == FulfillmentTaskStatus.Pending)
        {
            // Implicitly start the exact amount
            StartedQuantity += quantityToComplete;
            Status = FulfillmentTaskStatus.InProgress;
        }

        if (Status != FulfillmentTaskStatus.InProgress)
            return Result.Failure(Error.Validation("FulfillmentTask.NotInProgress", "Can only complete work on an InProgress task."));

        if (quantityToComplete <= 0)
            return Result.Failure(Error.Validation("FulfillmentTask.QuantityInvalid", "Completion quantity must be greater than zero."));

        int currentInProgress = StartedQuantity - CompletedQuantity - ScrappedQuantity;
        if (quantityToComplete > currentInProgress)
            return Result.Failure(Error.Validation("FulfillmentTask.ExceedsInProgress", "Cannot complete more items than are currently actively in progress."));

        CompletedQuantity += quantityToComplete;

        if (CompletedQuantity == RequestedQuantity)
        {
            Status = FulfillmentTaskStatus.Ready;
        }

        return Result.Success();
    }

    public Result MarkScrapped(int quantityToScrap)
    {
        if (Status != FulfillmentTaskStatus.InProgress)
            return Result.Failure(Error.Validation("FulfillmentTask.NotInProgress", "Can only scrap items on an InProgress task."));

        if (quantityToScrap <= 0)
            return Result.Failure(Error.Validation("FulfillmentTask.QuantityInvalid", "Scrap quantity must be greater than zero."));

        // Ensure we don't scrap more than what is actually physically in progress
        int currentInProgress = StartedQuantity - CompletedQuantity - ScrappedQuantity;
        if (quantityToScrap > currentInProgress)
            return Result.Failure(Error.Validation("FulfillmentTask.ExceedsInProgress", "Cannot scrap more items than are currently actively in progress."));

        ScrappedQuantity += quantityToScrap;

        return Result.Success();
    }

    public Result UnlinkOrderItem()
    {
        if (LinkedOrderItemId == null)
            return Result.Failure(Error.Validation("FulfillmentTask.NotLinked", "Task is not linked to any Order Item."));

        LinkedOrderItemId = null;
        return Result.Success();
    }

    public Result Cancel()
    {
        if (Status == FulfillmentTaskStatus.Ready)
            return Result.Failure(Error.Validation("FulfillmentTask.InvalidStatus", "Cannot cancel a completed task."));

        if (Status == FulfillmentTaskStatus.Cancelled)
            return Result.Success(); // Already cancelled

        Status = FulfillmentTaskStatus.Cancelled;
        return Result.Success();
    }
}
