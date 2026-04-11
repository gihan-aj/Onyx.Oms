namespace Onyx.Oms.Core.Domain.Enums;

public enum OrderItemStatus
{
    Allocated = 0,
    ToBeProduced = 1,
    ToBeProcured = 2,
    InProduction = 3,
    Ordered = 4, // Procurement
    Ready = 5
}
