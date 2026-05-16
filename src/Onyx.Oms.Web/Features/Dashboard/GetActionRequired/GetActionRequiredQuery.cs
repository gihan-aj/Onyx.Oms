using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Dashboard.GetActionRequired
{
    public record GetActionRequiredQuery(int Limit = 5) : IQuery<ActionRequiredListDto>;

    public record ActionRequiredListDto(
        int Total,
        List<ActionRequiredItemDto> Items);
    public record ActionRequiredItemDto(
        string Type,
        Guid OrderId,
        string? OrderNumber,
        string CustomerName,
        decimal TotalAmount,
        string Currency,
        string Status,
        string Reason,
        string ReasonLabel,
        DateTimeOffset? CreatedAt);
}
