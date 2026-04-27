using Onyx.Oms.Core.Common.Interfaces;

namespace Onyx.Oms.Core.Domain.Events;

public class FulfillmentTaskCompletedEvent : IDomainEvent
{
    public Guid TaskId { get; }
    public Guid OrderItemId { get; }
    public int CompletedQuantity { get; }

    public FulfillmentTaskCompletedEvent(Guid taskId, Guid orderItemId, int completedQuantity)
    {
        TaskId = taskId;
        OrderItemId = orderItemId;
        CompletedQuantity = completedQuantity;
    }
}
