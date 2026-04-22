using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Enums;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Orders.GetOrdersPaged
{
    public record GetOrdersPagedQuery : PagedRequest, IQuery<PagedResult<OrderSummaryDto>>
    {
        public OrderStatus? Status { get; init; }
        public PaymentStatus? PaymentStatus { get; init; }
        public Guid? CustomerId { get; init; }
        public DateTimeOffset? FromDate { get; init; }
        public DateTimeOffset? ToDate { get; init; }
        public bool? IncludeDetails { get; init; }
    }

    public record OrderSummaryDto(
        Guid Id,
        string OrderNumber,
        DateTimeOffset? OrderDate,
        Guid CustomerId,
        string CustomerName,
        string? CustomerEmail,
        string PrimaryPhone,
        OrderStatus Status,
        PaymentStatus PaymentStatus,
        decimal GrandTotalAmount,
        string GrandTotalCurrency,
        decimal TotalPaidAmount,
        decimal BalanceAmount,
        bool IsCashOnDelivery,
        string? TrackingNumber,
        List<OrderItemSummaryDto>? Items,
        List<OrderPaymentSummaryDto>? Payments,
        DateTimeOffset CreatedOnUtc,
        DateTimeOffset? LastModifiedOnUtc
    );

    public record OrderItemSummaryDto(
        Guid Id,
        Guid ProductVariantId,
        int Quantity,
        decimal UnitPriceAmount,
        string UnitPriceCurrency,
        decimal LineTotalAmount,
        OrderItemStatus Status
    );

    public record OrderPaymentSummaryDto(
        Guid Id,
        decimal Amount,
        string Currency,
        PaymentMethod Method,
        string? Reference,
        DateTime PaymentDate
    );
}
