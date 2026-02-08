using Asp.Versioning;
using MediatR;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.Customers.GetCustomerById;

public class GetCustomerByIdEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/v{version:apiVersion}/customers")
            .WithApiVersionSet(app.NewApiVersionSet("Customers").Build()) 
            .HasApiVersion(1);

        group.MapGet("{id:guid}", async (ISender sender, Guid id) =>
        {
            Result<CustomerDto> result = await sender.Send(new GetCustomerByIdQuery(id));

            return result.ToMinimalApiResult();
        })
        .WithTags("Customers")
        .WithName("GetCustomerById")
        .WithSummary("Get customer by ID")
        .WithDescription("Retrieves a customer's details by their unique identifier.");
    }
}
