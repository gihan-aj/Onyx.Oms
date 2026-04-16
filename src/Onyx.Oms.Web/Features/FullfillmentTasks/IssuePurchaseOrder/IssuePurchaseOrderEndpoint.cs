using FluentValidation;
using MediatR;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Constants;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.FullfillmentTasks.IssuePurchaseOrder
{
    public class IssuePurchaseOrderEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("api/v{version:apiVersion}/fulfillment-tasks")
                .WithApiVersionSet(app.NewApiVersionSet("FulfillmentTasks").Build())
                .HasApiVersion(1);

            group.MapPut("issue-purchase-order", async (ISender sender, IssuePurchaseOrderCommand command) =>
            {
                Result result = await sender.Send(command);

                return result.ToMinimalApiResult();
            })
            .WithTags("FulfillmentTasks")
            .WithName("IssuePurchaseOrder")
            .WithSummary("Issue a purcahse order for a procurement task")
            .WithDescription("Update the PO number for the task and Creates a new procurement task if PO doesn't issue all the required quantity.")
            .HasPermission(Permissions.FulfillmentTasks.Edit);
        }
    }
}
