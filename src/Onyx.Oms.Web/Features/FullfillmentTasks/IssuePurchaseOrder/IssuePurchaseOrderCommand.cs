using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.FullfillmentTasks.IssuePurchaseOrder
{
    public record IssuePurchaseOrderCommand(
        Guid ProcurementTaskId, 
        int IssueQuantity, 
        string PurchaseOrderNumber, 
        MoneyDto Cost) : ICommand;

    public record MoneyDto(decimal Amount, string Currency = "LKR");
}
