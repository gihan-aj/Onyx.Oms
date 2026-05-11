using MediatR;
using Microsoft.AspNetCore.Mvc;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Constants;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.Orders.CancelOrder
{
    public class CancelOrderEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("api/v{version:apiVersion}/orders")
                .WithApiVersionSet(app.NewApiVersionSet("Orders").Build())
                .HasApiVersion(1);

            group.MapPost("{id}/cancel", async (Guid id, [FromBody]CancelOrderRequest request, ISender sender) =>
            {
                Result result = await sender.Send(new CancelOrderCommand(id, request.Reason));
                return result.ToMinimalApiResult();
            })
            .WithTags("Orders")
            .WithName("CancelOrder")
            .WithSummary("Cancel an order")
            .WithDescription("Cancels an order and raises an event to handle side effects.")
            .HasPermission(Permissions.Orders.Edit);
        }
    }

    public record CancelOrderRequest(string? Reason);
}
