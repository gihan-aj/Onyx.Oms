using MediatR;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Constants;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.FullfillmentTasks.CancelProductionTask;

public class CancelProductionTaskEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/v{version:apiVersion}/fulfillment-tasks")
            .WithApiVersionSet(app.NewApiVersionSet("FulfillmentTasks").Build())
            .HasApiVersion(1);

        group.MapPut("cancel-production", async (ISender sender, CancelProductionTaskCommand command) =>
        {
            Result result = await sender.Send(command);
            return result.ToMinimalApiResult();
        })
        .WithTags("FulfillmentTasks")
        .WithName("CancelProductionTask")
        .WithSummary("Cancel a production task")
        .WithDescription("Cancels an existing production task if it is not already completed.")
        .HasPermission(Permissions.FulfillmentTasks.Edit);
    }
}
