using MediatR;
using Microsoft.AspNetCore.Mvc;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Constants;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.Orders.GetOrderById
{
    public class GetOrderByIdEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("api/v{version:apiVersion}/orders")
                .WithApiVersionSet(app.NewApiVersionSet("Orders").Build())
                .HasApiVersion(1);

            group.MapGet("{id}", async (Guid id, ISender sender) =>
            {
                var query = new GetOrderByIdQuery(id);
                var result = await sender.Send(query);
                return result.ToMinimalApiResult();
            })
            .WithTags("Orders")
            .WithName("GetOrderById")
            .WithSummary("Get order by ID")
            .WithDescription("Retrieves the details of a specific order, including customer, items, and payments.")
            .Produces<OrderDetailsDto>()
            .HasPermission(Permissions.Orders.View);
        }
    }
}
