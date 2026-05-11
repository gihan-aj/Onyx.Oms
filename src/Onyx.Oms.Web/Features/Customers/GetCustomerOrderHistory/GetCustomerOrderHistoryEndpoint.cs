using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Onyx.Oms.Core.Domain.Constants;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.Customers.GetCustomerOrderHistory
{
    public class GetCustomerOrderHistoryEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("api/v{version:apiVersion}/customers")
                .WithApiVersionSet(app.NewApiVersionSet("Customers").Build())
                .HasApiVersion(1);
            group.MapGet("{id}/orders", async (Guid id, [FromQuery] int? top, ISender sender) =>
            {
                int limit = top ?? 20; // Default to 20 if ?top= is not provided
                var query = new GetCustomerOrderHistoryQuery(id, limit);
                var result = await sender.Send(query);
                return result.ToMinimalApiResult();
            })
                .WithTags("Customers")
                .WithName("GetCustomerOrderHistory")
                .WithSummary("Get customer order history")
                .WithDescription("Retrieves the total order count and recent order history for a specific customer.")
                .Produces<CustomerOrderHistoryResponse>()
                .HasPermission(Permissions.Customers.View);
        }
    }
}
