using MediatR;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Constants;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.FullfillmentTasks.GetFulfillmentTasksPaged
{
    public class GetFullfillmentTasksPagedEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("api/v{version:apiVersion}/fulfillment-tasks")
                .WithApiVersionSet(app.NewApiVersionSet("FulfillmentTasks").Build())
                .HasApiVersion(1);

            group.MapGet("search", async (ISender sender, [AsParameters] GetFullfillmentTasksPagedQuery query) =>
            {
                Result<PagedResult<FulfillmentTaskDto>> result = await sender.Send(query);

                return result.ToMinimalApiResult();
            })
            .WithTags("FulfillmentTasks")
            .WithName("GetFulfillmentTasksPaged")
            .WithSummary("Search fulfillment tasks")
            .WithDescription("Retrieves a paginated list of fulfillment tasks with optional searching and sorting.")
            .Produces<FulfillmentTaskDto>()
            .HasPermission(Permissions.FulfillmentTasks.View);
        }
    }
}
