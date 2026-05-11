using Onyx.Oms.Core.Domain.Enums;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Customers.GetCustomerOrderHistory
{
    public record GetCustomerOrderHistoryQuery(Guid CustomerId, int Top = 20) : IQuery<CustomerOrderHistoryResponse>;


    public record CustomerOrderHistoryResponse(
        int TotalOrdersCount,
        List<CustomerOrderSummaryDto> RecentOrders
    );

    public record CustomerOrderSummaryDto(
        Guid Id,
        string OrderNumber,
        DateTimeOffset? OrderDate,
        OrderStatus Status,
        PaymentStatus PaymentStatus,
        decimal GrandTotalAmount,
        string GrandTotalCurrency,
        decimal BalanceAmount
    );
}
