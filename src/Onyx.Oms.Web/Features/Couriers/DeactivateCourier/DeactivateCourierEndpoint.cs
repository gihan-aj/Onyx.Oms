using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.Couriers.DeactivateCourier;

public class DeactivateCourierEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/v{version:apiVersion}/couriers")
            .WithApiVersionSet(app.NewApiVersionSet("Couriers").Build()) 
            .HasApiVersion(1);

        group.MapPut("{id:guid}/deactivate", async (ISender sender, Guid id) =>
        {
            Result result = await sender.Send(new DeactivateCourierCommand(id));

            if(result.IsSuccess)
            {
                return Results.NoContent();
            }

            return result.ToProblemDetails();
        })
        .WithTags("Couriers")
        .WithName("DeactivateCourier")
        .WithSummary("Deactivate a courier")
        .WithDescription("Deactivates a courier account.");
    }
}
