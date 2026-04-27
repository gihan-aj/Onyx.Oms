using MediatR;
using Microsoft.AspNetCore.Mvc;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Constants;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.Orders.DeliverOrder
{
    public class DeliverOrderEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("api/v{version:apiVersion}/orders")
                .WithApiVersionSet(app.NewApiVersionSet("Orders").Build())
                .HasApiVersion(1);

            group.MapPost("{id}/deliver", async (Guid id, ISender sender) =>
            {
                Result result = await sender.Send(new DeliverOrderCommand(id));
                return result.ToMinimalApiResult();
            })
            .WithTags("Orders")
            .WithName("DeliverOrder")
            .WithSummary("Deliver an order")
            .WithDescription("Transitions a Shipped order to Delivered.")
            .HasPermission(Permissions.Orders.Edit);
        }
    }
}
