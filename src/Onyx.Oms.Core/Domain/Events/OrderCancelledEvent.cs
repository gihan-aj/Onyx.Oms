using Onyx.Oms.Core.Common.Interfaces;

namespace Onyx.Oms.Core.Domain.Events;

public class OrderCancelledEvent : IDomainEvent
{
    public Guid OrderId { get; }

    public OrderCancelledEvent(Guid orderId)
    {
        OrderId = orderId;
    }
}
