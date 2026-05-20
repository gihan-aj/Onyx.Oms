namespace Onyx.Oms.Core.Domain.Enums;

public enum OrderStatus
{
    Pending = 0,
    Confirmed = 1,
    Processing = 2,
    ReadyToPack = 3,
    Packed = 4,
    Shipped = 5,
    Delivered = 6,
    Completed = 7,
    PaymentFailed = 8,
    Cancelled = 9,
    ReturnInTransit = 10,
    ReturnedToSender = 11,
    ReturnProcessed = 12,
    LostInTransit = 13,
    DeliveryFailed = 14,
}
