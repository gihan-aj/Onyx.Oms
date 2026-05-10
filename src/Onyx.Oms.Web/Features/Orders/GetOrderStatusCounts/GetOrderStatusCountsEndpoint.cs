using MediatR;
using Microsoft.AspNetCore.Mvc;
using Onyx.Oms.Core.Domain.Constants;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.Orders.GetOrderStatusCounts
{
    public class GetOrderStatusCountsEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("api/v{version:apiVersion}/orders")
                .WithApiVersionSet(app.NewApiVersionSet("Orders").Build())
                .HasApiVersion(1);

            group.MapGet("status-counts", async ([AsParameters] GetOrderStatusCountsQuery query, ISender sender) =>
            {
                var result = await sender.Send(query);
                return result.ToMinimalApiResult();
            })
            .WithTags("Orders")
            .WithName("GetOrderStatusCounts")
            .WithSummary("Get order counts by status")
            .WithDescription("Retrieves the count of orders grouped by their current status based on the provided filters.")
            .Produces<GetOrderStatusCountsResponse>()
            .HasPermission(Permissions.Orders.View);
        }
    }
}
