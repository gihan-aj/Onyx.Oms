using MediatR;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Constants;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.Customers.GetCustomersPaged;

public class GetCustomersPagedEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/v{version:apiVersion}/customers")
            .WithApiVersionSet(app.NewApiVersionSet("Customers").Build()) 
            .HasApiVersion(1);

        group.MapGet("search", async (ISender sender, [AsParameters] GetCustomersPagedQuery query) =>
        {
            Result<PagedResult<CustomerDto>> result = await sender.Send(query);

            return result.ToMinimalApiResult();
        })
        .WithTags("Customers")
        .WithName("GetCustomersPaged")
        .WithSummary("Search customers")
        .WithDescription("Retrieves a paginated list of customers with optional searching (Name, Email, Phone) and sorting.")
        .HasPermission(Permissions.Customers.View);
    }
}
