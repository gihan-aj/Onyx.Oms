using MediatR;
using Microsoft.AspNetCore.Mvc;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Constants;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.Orders.RollbackOrderToPending
{
    public class RollbackOrderToPendingEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("api/v{version:apiVersion}/orders")
                .WithApiVersionSet(app.NewApiVersionSet("Orders").Build())
                .HasApiVersion(1);
            group.MapPost("{id}/rollback-to-pending", async (Guid id, [FromBody] RollbackOrderToPendingRequest request, ISender sender) =>
            {
                Result result = await sender.Send(new RollbackOrderToPendingCommand(id, request.Reason));
                return result.ToMinimalApiResult();
            })
            .WithTags("Orders")
            .WithName("RollbackOrderToPending")
            .WithSummary("Rollback an order to Pending")
            .WithDescription("Transitions a Ready To Pack, Processing or Confirmed order back to Pending and records the reason for rollback.")
            .HasPermission(Permissions.Orders.Edit);
        }
    }

    public record RollbackOrderToPendingRequest(string Reason);
}
