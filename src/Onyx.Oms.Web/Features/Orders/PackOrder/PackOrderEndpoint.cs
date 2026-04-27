using MediatR;
using Microsoft.AspNetCore.Mvc;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Constants;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.Orders.PackOrder
{
    public class PackOrderEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("api/v{version:apiVersion}/orders")
                .WithApiVersionSet(app.NewApiVersionSet("Orders").Build())
                .HasApiVersion(1);

            group.MapPost("{id}/pack", async (Guid id, ISender sender) =>
            {
                Result result = await sender.Send(new PackOrderCommand(id));
                return result.ToMinimalApiResult();
            })
            .WithTags("Orders")
            .WithName("PackOrder")
            .WithSummary("Pack an order")
            .WithDescription("Transitions a ReadyToPack order to Packed.")
            .HasPermission(Permissions.Orders.Edit);
        }
    }
}
