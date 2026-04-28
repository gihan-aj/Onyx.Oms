using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Orders.GetOrderById
{
    public record GetOrderByIdQuery(Guid OrderId) : IQuery<OrderDetailsDto>;
}
