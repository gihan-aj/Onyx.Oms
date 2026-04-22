using MediatR;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Constants;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.Orders.CreateOrder
{
    public class CreateOrderEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("api/v{version:apiVersion}/orders")
                .WithApiVersionSet(app.NewApiVersionSet("Orders").Build())
                .HasApiVersion(1);

            group.MapPost("", async (ISender sender, CreateOrderCommand command) =>
            {
                Result<Guid> result = await sender.Send(command);

                return result.ToMinimalApiResult();
            })
            .WithTags("Orders")
            .WithName("CreateOrder")
            .WithSummary("Create a new order")
            .WithDescription("Creates a new sales order.")
            .HasPermission(Permissions.Orders.Create);
        }
    }
}
