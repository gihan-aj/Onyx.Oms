using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Dashboard.GetInMotion
{
    public record GetInMotionQuery(int Limit = 5) : IQuery<InMotionListDto>;

    public record InMotionListDto(
        int Total,
        List<InMotionItemDto> Items);

    public record InMotionItemDto(
        string Type,
        Guid? TaskId,
        string? VariantLabel,
        string? TaskType,
        string? TaskStatus,
        int? Quantity,
        string? LinkedOrderNumber,
        Guid? LinkedOrderId,
        bool? IsOrphaned,
        Guid? OrderId,
        string? OrderNumber,
        string? CustomerName,
        string? OrderStatus,
        string? TrackingNumber,
        string ContextLabel);
}
