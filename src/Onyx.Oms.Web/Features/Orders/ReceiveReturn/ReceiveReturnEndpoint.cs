using MediatR;
using Microsoft.AspNetCore.Mvc;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Constants;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.Orders.ReceiveReturn
{
    public class ReceiveReturnEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("api/v{version:apiVersion}/orders")
                .WithApiVersionSet(app.NewApiVersionSet("Orders").Build())
                .HasApiVersion(1);
            group.MapPost("{id}/receive-return", async (Guid id, [FromBody] ReceiveReturnRequest request, ISender sender) =>
            {
                Result result = await sender.Send(new ReceiveReturnCommand(id, request.IsReceived, request.Reason));
                return result.ToMinimalApiResult();
            })
            .WithTags("Orders")
            .WithName("ReceiveReturn")
            .WithSummary("Receive a returned order")
            .WithDescription("Transitions a ReturnInTransit order to ReturnedToSender or LostInTransit.")
            .HasPermission(Permissions.Orders.Edit);
        }
    }
}
