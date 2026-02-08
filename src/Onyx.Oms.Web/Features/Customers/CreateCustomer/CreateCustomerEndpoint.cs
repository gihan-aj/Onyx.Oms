using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.Customers.CreateCustomer;

public class CreateCustomerEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/v{version:apiVersion}/customers")
            .WithApiVersionSet(app.NewApiVersionSet("Customers").Build()) 
            .HasApiVersion(1);

        group.MapPost("", async (ISender sender, [FromBody] CreateCustomerCommand command) =>
        {
            Result<Guid> result = await sender.Send(command);

            return result.ToMinimalApiResult();
        })
        .WithTags("Customers")
        .WithName("CreateCustomer")
        .WithSummary("Create a new customer")
        .WithDescription("Creates a new customer record with address.");
    }
}
