using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Constants;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.Customers.UpdateCustomer;

public class UpdateCustomerEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/v{version:apiVersion}/customers")
            .WithApiVersionSet(app.NewApiVersionSet("Customers").Build()) 
            .HasApiVersion(1);

        group.MapPut("{id:guid}", async (ISender sender, Guid id, [FromBody] UpdateCustomerCommand command) =>
        {
            if (id != command.Id)
            {
                return Results.Problem(statusCode: 400, title: "Bad Request", detail: "ID in path does not match ID in body.");
            }

            Result result = await sender.Send(command);

            return result.ToMinimalApiResult();
        })
        .WithTags("Customers")
        .WithName("UpdateCustomer")
        .WithSummary("Update a customer")
        .WithDescription("Updates an existing customer's details.")
        .HasPermission(Permissions.Customers.Edit);
    }
}
