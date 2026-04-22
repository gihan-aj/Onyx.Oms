using MediatR;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Constants;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.Orders.GetOrdersPaged
{
    public class GetOrdersPagedEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("api/v{version:apiVersion}/orders")
                .WithApiVersionSet(app.NewApiVersionSet("Orders").Build())
                .HasApiVersion(1);

            group.MapGet("", async (ISender sender, [AsParameters] GetOrdersPagedQuery query) =>
            {
                Result<PagedResult<OrderSummaryDto>> result = await sender.Send(query);

                return result.ToMinimalApiResult();
            })
            .WithTags("Orders")
            .WithName("GetOrdersPaged")
            .WithSummary("Get a paginated list of orders")
            .WithDescription("Retrieves orders with filtering, sorting, and pagination.")
            .HasPermission(Permissions.Orders.View);
        }
    }
}
