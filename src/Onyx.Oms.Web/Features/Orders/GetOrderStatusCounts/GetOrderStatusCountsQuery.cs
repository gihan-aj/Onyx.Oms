using Onyx.Oms.Core.Domain.Enums;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Orders.GetOrderStatusCounts
{
    public record GetOrderStatusCountsQuery : IQuery<GetOrderStatusCountsResponse>
    {
        public string? SearchTerm { get; init; }
        public PaymentStatus? PaymentStatus { get; init; }
        public Guid? CustomerId { get; init; }
        public DateTimeOffset? FromDate { get; init; }
        public DateTimeOffset? ToDate { get; init; }
        public bool? IsCashOnDelivery { get; init; }
        public Guid? CourierId { get; init; }
    }

    public record OrderStatusCountDto(OrderStatus Status, int Count);

    public record GetOrderStatusCountsResponse(
        List<OrderStatusCountDto> Counts,
        int TotalCount
    );
}
