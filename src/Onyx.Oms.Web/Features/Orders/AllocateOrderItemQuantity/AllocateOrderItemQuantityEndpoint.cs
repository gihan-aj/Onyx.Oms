using MediatR;
using Microsoft.AspNetCore.Mvc;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Constants;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.Orders.AllocateOrderItemQuantity
{
    public class AllocateOrderItemQuantityEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("api/v{version:apiVersion}/orders")
                .WithApiVersionSet(app.NewApiVersionSet("Orders").Build())
                .HasApiVersion(1);

            group.MapPut("{orderId:guid}/items/{orderItemId:guid}/allocate-quantity",
            async (
                Guid orderId,
                Guid orderItemId,
                [FromBody] AllocateOrderItemQuantityRequest request,
                ISender sender) =>
            {
                var command = new AllocateOrderItemQuantityCommand(
                    orderId,
                    orderItemId,
                    request.QuantityToAllocate);

                Result result = await sender.Send(command);

                return result.ToMinimalApiResult();
            })
                .WithTags("Orders")
                .WithName("AllocateOrderItemQuantity")
                .WithSummary("Allocate quantity to order item")
                .WithDescription("Allocate from available quantity for an order item in an order.")
                .HasPermission(Permissions.Orders.Edit);
        }
    }
}
