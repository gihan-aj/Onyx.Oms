using MediatR;
using Microsoft.AspNetCore.Mvc;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Constants;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.Orders.CompleteOrder
{
    public class CompleteOrderEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("api/v{version:apiVersion}/orders")
                .WithApiVersionSet(app.NewApiVersionSet("Orders").Build())
                .HasApiVersion(1);

            group.MapPost("{id}/complete", async (Guid id, ISender sender) =>
            {
                Result result = await sender.Send(new CompleteOrderCommand(id));
                return result.ToMinimalApiResult();
            })
            .WithTags("Orders")
            .WithName("CompleteOrder")
            .WithSummary("Complete an order")
            .WithDescription("Transitions a Delivered order to Completed. Requires the order to be fully paid.")
            .HasPermission(Permissions.Orders.Edit);
        }
    }
}
