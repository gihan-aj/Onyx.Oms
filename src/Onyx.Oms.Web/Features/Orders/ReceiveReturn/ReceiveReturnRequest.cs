namespace Onyx.Oms.Web.Features.Orders.ReceiveReturn
{
    public record ReceiveReturnRequest(bool IsReceived, string? Reason);
}
