using MediatR;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Constants;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.FullfillmentTasks.GetFulfillmentTaskById;

public class GetFulfillmentTaskByIdEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/v{version:apiVersion}/fulfillment-tasks")
            .WithApiVersionSet(app.NewApiVersionSet("FulfillmentTasks").Build())
            .HasApiVersion(1);

        group.MapGet("{id:guid}", async (Guid id, ISender sender) =>
        {
            var query = new GetFulfillmentTaskByIdQuery(id);
            Result<FulfillmentTaskByIdDto> result = await sender.Send(query);
            return result.ToMinimalApiResult();
        })
        .WithTags("FulfillmentTasks")
        .WithName("GetFulfillmentTaskById")
        .WithSummary("Get fulfillment task by id")
        .WithDescription("Retrieves the details of a single fulfillment task.")
        .HasPermission(Permissions.FulfillmentTasks.View);
    }
}
