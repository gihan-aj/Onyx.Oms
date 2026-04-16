using FluentValidation;
using MediatR;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Constants;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.FullfillmentTasks.StartProduction
{
    public class StartProductionEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("api/v{version:apiVersion}/fulfillment-tasks")
                .WithApiVersionSet(app.NewApiVersionSet("FulfillmentTasks").Build())
                .HasApiVersion(1);

            group.MapPut("start-production", async (ISender sender, StartProductionCommand command) =>
            {
                Result result = await sender.Send(command);

                return result.ToMinimalApiResult();
            })
            .WithTags("FulfillmentTasks")
            .WithName("StartProduction")
            .WithSummary("Start production task")
            .WithDescription("Start working on some or all of the requested quantity of a production task.")
            .HasPermission(Permissions.FulfillmentTasks.Edit);
        }
    }
}
