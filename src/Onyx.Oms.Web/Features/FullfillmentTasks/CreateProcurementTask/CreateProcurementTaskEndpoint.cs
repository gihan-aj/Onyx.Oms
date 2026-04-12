using MediatR;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Constants;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.FullfillmentTasks.CreateProcurementTask
{
    public class CreateProcurementTaskEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("api/v{version:apiVersion}/fulfillment-tasks/procurement")
                .WithApiVersionSet(app.NewApiVersionSet("FulfillmentTasks").Build())
                .HasApiVersion(1);

            group.MapPost("", async (ISender sender, CreateProcurementTaskCommand command) =>
            {
                Result<Guid> result = await sender.Send(command);

                return result.ToMinimalApiResult();
            })
            .WithTags("FulfillmentTasks")
            .WithName("CreateProcurementTask")
            .WithSummary("Create a new procurement task")
            .WithDescription("Creates a new procurement task for a product variant.")
            .Produces<Guid>()
            .HasPermission(Permissions.FulfillmentTasks.Create);
        }
    }
}
