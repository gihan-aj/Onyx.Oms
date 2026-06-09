using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Constants;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.Orders.UnpackOrder
{
    public class UnpackOrderEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("api/v{version:apiVersion}/orders")
                .WithApiVersionSet(app.NewApiVersionSet("Orders").Build())
                .HasApiVersion(1);
            group.MapPost("{id}/unpack", async (Guid id, [FromBody] UnpackRequest request, ISender sender) =>
            {
                Result result = await sender.Send(new UnpackOrderCommand(id, request.Reason));
                return result.ToMinimalApiResult();
            })
            .WithTags("Orders")
            .WithName("Unpack")
            .WithSummary("Unpack an order")
            .WithDescription("Transitions a Shipped order back to Ready to Pack and records the reason for rollback.")
            .HasPermission(Permissions.Orders.Edit);
        }
    }

    public record UnpackRequest(string Reason);
}
