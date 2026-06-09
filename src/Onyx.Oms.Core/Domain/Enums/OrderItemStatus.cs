namespace Onyx.Oms.Core.Domain.Enums;

public enum OrderItemStatus
{
    Allocated = 0,
    Pending = 1,
    InProduction = 2,
    Ordered = 3, // Procurement
    Ready = 4,
    ToBeProduced = 5,
    ToBeProcured = 6
}
