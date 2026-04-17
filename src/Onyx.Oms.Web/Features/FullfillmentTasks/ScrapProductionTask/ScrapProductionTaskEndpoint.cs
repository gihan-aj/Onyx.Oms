using FluentValidation;
using MediatR;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Constants;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.FullfillmentTasks.ScrapProductionTask;

public class ScrapProductionTaskEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/v{version:apiVersion}/fulfillment-tasks")
            .WithApiVersionSet(app.NewApiVersionSet("FulfillmentTasks").Build())
            .HasApiVersion(1);

        group.MapPut("scrap-production", async (ISender sender, ScrapProductionTaskCommand command) =>
        {
            Result result = await sender.Send(command);
            return result.ToMinimalApiResult();
        })
        .WithTags("FulfillmentTasks")
        .WithName("ScrapProductionTask")
        .WithSummary("Scrap a quantity of a production task")
        .WithDescription("Marks some in-progress quantity of a production task as scrapped.")
        .HasPermission(Permissions.FulfillmentTasks.Edit);
    }
}
