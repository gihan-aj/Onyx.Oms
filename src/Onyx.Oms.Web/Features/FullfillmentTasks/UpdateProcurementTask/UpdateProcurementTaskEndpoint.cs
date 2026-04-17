using MediatR;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Constants;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.FullfillmentTasks.UpdateProcurementTask;

public class UpdateProcurementTaskEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/v{version:apiVersion}/fulfillment-tasks")
            .WithApiVersionSet(app.NewApiVersionSet("FulfillmentTasks").Build())
            .HasApiVersion(1);

        group.MapPut("update-procurement", async (ISender sender, UpdateProcurementTaskCommand command) =>
        {
            Result result = await sender.Send(command);
            return result.ToMinimalApiResult();
        })
        .WithTags("FulfillmentTasks")
        .WithName("UpdateProcurementTask")
        .WithSummary("Update a procurement task")
        .WithDescription("Updates the details of a procurement task.")
        .HasPermission(Permissions.FulfillmentTasks.Edit);
    }
}
