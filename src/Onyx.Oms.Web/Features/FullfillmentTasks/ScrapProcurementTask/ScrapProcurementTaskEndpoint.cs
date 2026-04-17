using FluentValidation;
using MediatR;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Constants;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.FullfillmentTasks.ScrapProcurementTask;

public class ScrapProcurementTaskEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/v{version:apiVersion}/fulfillment-tasks")
            .WithApiVersionSet(app.NewApiVersionSet("FulfillmentTasks").Build())
            .HasApiVersion(1);

        group.MapPut("scrap-procurement", async (ISender sender, ScrapProcurementTaskCommand command) =>
        {
            Result result = await sender.Send(command);
            return result.ToMinimalApiResult();
        })
        .WithTags("FulfillmentTasks")
        .WithName("ScrapProcurementTask")
        .WithSummary("Scrap a quantity of a procurement task")
        .WithDescription("Marks some in-progress quantity of a procurement task as scrapped.")
        .HasPermission(Permissions.FulfillmentTasks.Edit);
    }
}
