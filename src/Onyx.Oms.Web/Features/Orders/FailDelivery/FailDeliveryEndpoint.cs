using MediatR;
using Microsoft.AspNetCore.Mvc;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Constants;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.Orders.FailDelivery
{
    public class FailDeliveryEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("api/v{version:apiVersion}/orders")
                .WithApiVersionSet(app.NewApiVersionSet("Orders").Build())
                .HasApiVersion(1);

            group.MapPost("{id}/fail-delivery", async (Guid id, [FromBody] FailDeliveryRequest request, ISender sender) =>
            {
                Result result = await sender.Send(new FailDeliveryCommand(id, request.IsReturnedToSender, request.Reason));
                return result.ToMinimalApiResult();
            })
            .WithTags("Orders")
            .WithName("FailDelivery")
            .WithSummary("Fail a delivery")
            .WithDescription("Transitions a Shipped order to DeliveryFailed or ReturnedToSender.")
            .HasPermission(Permissions.Orders.Edit);
        }
    }

    public record FailDeliveryRequest(bool IsReturnedToSender, string? Reason);
}
