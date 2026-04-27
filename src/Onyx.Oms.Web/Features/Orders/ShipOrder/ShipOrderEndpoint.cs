using MediatR;
using Microsoft.AspNetCore.Mvc;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Constants;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.Orders.ShipOrder
{
    public class ShipOrderEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("api/v{version:apiVersion}/orders")
                .WithApiVersionSet(app.NewApiVersionSet("Orders").Build())
                .HasApiVersion(1);

            group.MapPost("{id}/ship", async (Guid id, [FromBody] ShipOrderRequest request, ISender sender) =>
            {
                Result result = await sender.Send(new ShipOrderCommand(id, request.CourierId, request.TrackingNumber));
                return result.ToMinimalApiResult();
            })
            .WithTags("Orders")
            .WithName("ShipOrder")
            .WithSummary("Ship an order")
            .WithDescription("Transitions a Packed order to Shipped and records the Courier and Tracking Number.")
            .HasPermission(Permissions.Orders.Edit);
        }
    }

    public record ShipOrderRequest(Guid CourierId, string? TrackingNumber);
}
