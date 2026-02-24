using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Constants;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;
using Onyx.Oms.Web.Features.Couriers.CreateCourier;

namespace Onyx.Oms.Web.Features.Couriers;

public class CreateCourierEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/v{version:apiVersion}/couriers")
            .WithApiVersionSet(app.NewApiVersionSet("Couriers").Build()) // Or use a shared set
            .HasApiVersion(1);

        group.MapPost("", async (ISender sender, [FromBody] CreateCourierCommand command) =>
        {
            Result<Guid> result = await sender.Send(command);

            return result.ToMinimalApiResult();
        })
        .WithTags("Couriers")
        .WithName("CreateCourier")
        .WithSummary("Create a new courier")
        .WithDescription("Creates a new courier record. Name must be unique.")
        .HasPermission(Permissions.Couriers.Create);
    }
}
