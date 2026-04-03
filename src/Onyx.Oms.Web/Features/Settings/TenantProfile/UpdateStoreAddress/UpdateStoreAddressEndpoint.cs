using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.Settings.TenantProfile.UpdateStoreAddress;

public class UpdateStoreAddressEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/v{version:apiVersion}/settings/profile")
            .WithApiVersionSet(app.NewApiVersionSet("TenantProfile").Build()) 
            .HasApiVersion(1);

        group.MapPut("address", async ([FromBody] UpdateStoreAddressCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.ToMinimalApiResult();
        })
        .WithTags("Settings")
        .WithName("UpdateStoreAddress")
        .WithSummary("Update store address")
        .WithDescription("Updates the physical store address for the tenant profile.")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .RequireAuthorization()
        .HasPermission(Onyx.Oms.Core.Domain.Constants.Permissions.Tenants.Edit);
    }
}
