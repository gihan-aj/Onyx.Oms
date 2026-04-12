using FluentValidation;
using MediatR;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Constants;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.FullfillmentTasks.CreateProductionTask
{
    public class CreateProductionTaskEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            // Note the /production path to avoid colliding with the procurement POST
            var group = app.MapGroup("api/v{version:apiVersion}/fulfillment-tasks/production")
                .WithApiVersionSet(app.NewApiVersionSet("FulfillmentTasks").Build())
                .HasApiVersion(1);

            group.MapPost("", async (ISender sender, CreateProductionTaskCommand command) =>
            {
                Result<Guid> result = await sender.Send(command);

                return result.ToMinimalApiResult();
            })
            .WithTags("FulfillmentTasks")
            .WithName("CreateProductionTask")
            .WithSummary("Create a new production task")
            .WithDescription("Creates a new internal production task for a product variant.")
            .Produces<Guid>()
            .HasPermission(Permissions.FulfillmentTasks.Create);
        }
    }
}
