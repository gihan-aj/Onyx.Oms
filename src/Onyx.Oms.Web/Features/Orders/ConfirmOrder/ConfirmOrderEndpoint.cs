using MediatR;
using Microsoft.AspNetCore.Mvc;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Constants;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.Orders.ConfirmOrder
{
    public class ConfirmOrderEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("api/v{version:apiVersion}/orders")
                .WithApiVersionSet(app.NewApiVersionSet("Orders").Build())
                .HasApiVersion(1);

            group.MapPost("{id}/confirm", async (Guid id, ISender sender) =>
            {
                Result result = await sender.Send(new ConfirmOrderCommand(id));
                return result.ToMinimalApiResult();
            })
            .WithTags("Orders")
            .WithName("ConfirmOrder")
            .WithSummary("Confirm an order")
            .WithDescription("Transitions a pending order to confirmed.")
            .HasPermission(Permissions.Orders.Edit);
        }
    }
}
