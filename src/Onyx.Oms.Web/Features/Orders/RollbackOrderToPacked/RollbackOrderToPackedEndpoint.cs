using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Constants;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.Orders.RollbackOrderToPacked
{
    public class RollbackOrderToPackedEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("api/v{version:apiVersion}/orders")
                .WithApiVersionSet(app.NewApiVersionSet("Orders").Build())
                .HasApiVersion(1);
            group.MapPost("{id}/rollback-to-packed", async (Guid id, [FromBody] RollbackOrderToPackedRequest request, ISender sender) =>
            {
                Result result = await sender.Send(new RollbackOrderToPackedCommand(id, request.Reason));
                return result.ToMinimalApiResult();
            })
            .WithTags("Orders")
            .WithName("RollbackOrderToPacked")
            .WithSummary("Rollback an order to Packed")
            .WithDescription("Transitions a Shipped order back to Packed and records the reason for rollback.")
            .HasPermission(Permissions.Orders.Edit);
        }
    }

    public record RollbackOrderToPackedRequest(string Reason);

}
