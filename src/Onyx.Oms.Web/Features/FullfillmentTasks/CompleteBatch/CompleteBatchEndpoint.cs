using MediatR;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Constants;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.FullfillmentTasks.CompleteBatch
{
    public class CompleteBatchEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("api/v{version:apiVersion}/fulfillment-tasks")
                .WithApiVersionSet(app.NewApiVersionSet("FulfillmentTasks").Build())
                .HasApiVersion(1);

            group.MapPut("complete-batch", async (ISender sender, CompleteBatchCommand command) =>
            {
                Result result = await sender.Send(command);
                return result.ToMinimalApiResult();
            })
            .WithTags("FulfillmentTasks")
            .WithName("CompleteBatch")
            .WithSummary("Complete a batch of tasks")
            .WithDescription("Marks all in-progress quantity of a product variant, Completed.")
            .HasPermission(Permissions.FulfillmentTasks.Edit);
        }
    }
}
