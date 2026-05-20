namespace Onyx.Oms.Web.Features.Orders.ProcessReturn
{
    public record ProcessReturnRequest(List<ReturnItemQuantity> ItemsToReturn, string? Reason);
}
