using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.Couriers.ActivateCourier;

public class ActivateCourierEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/v{version:apiVersion}/couriers")
            .WithApiVersionSet(app.NewApiVersionSet("Couriers").Build()) 
            .HasApiVersion(1);

        group.MapPut("{id:guid}/activate", async (ISender sender, Guid id) =>
        {
            Result result = await sender.Send(new ActivateCourierCommand(id));

            return result.ToMinimalApiResult();
        })
        .WithTags("Couriers")
        .WithName("ActivateCourier")
        .WithSummary("Activate a courier")
        .WithDescription("Activates a courier account.");
    }
}
