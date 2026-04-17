using FluentValidation;
using MediatR;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Constants;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.FullfillmentTasks.CompleteProcurementTask;

public class CompleteProcurementTaskEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/v{version:apiVersion}/fulfillment-tasks")
            .WithApiVersionSet(app.NewApiVersionSet("FulfillmentTasks").Build())
            .HasApiVersion(1);

        group.MapPut("complete-procurement", async (ISender sender, CompleteProcurementTaskCommand command) =>
        {
            Result result = await sender.Send(command);
            return result.ToMinimalApiResult();
        })
        .WithTags("FulfillmentTasks")
        .WithName("CompleteProcurementTask")
        .WithSummary("Complete a procurement task")
        .WithDescription("Marks some or all in-progress quantity of a procurement task as ready/completed.")
        .HasPermission(Permissions.FulfillmentTasks.Edit);
    }
}
