using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.Couriers.UpdateCourier;

public class UpdateCourierEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/v{version:apiVersion}/couriers")
            .WithApiVersionSet(app.NewApiVersionSet("Couriers").Build()) 
            .HasApiVersion(1);

        group.MapPut("{id:guid}", async (ISender sender, Guid id, [FromBody] UpdateCourierCommand command) =>
        {
            if (id != command.Id)
            {
                return Results.Problem(statusCode: 400, title: "Bad Request", detail: "ID in path does not match ID in body.");
            }

            Result result = await sender.Send(command);

            return result.ToMinimalApiResult();
        })
        .WithTags("Couriers")
        .WithName("UpdateCourier")
        .WithSummary("Update a courier")
        .WithDescription("Updates an existing courier. Checks for name uniqueness if name is changed.");
    }
}
