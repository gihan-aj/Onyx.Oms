using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.Settings.TenantProfile.UpdateStoreInfo;

public class UpdateStoreInfoEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/v{version:apiVersion}/settings/profile")
            .WithApiVersionSet(app.NewApiVersionSet("TenantProfile").Build()) 
            .HasApiVersion(1);

        group.MapPut("store-info", async ([FromBody] UpdateStoreInfoCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.ToMinimalApiResult();
        })
        .WithTags("Settings")
        .WithName("UpdateStoreInfo")
        .WithSummary("Update store info")
        .WithDescription("Updates basic store information such as business name and contact details.")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .RequireAuthorization()
        .HasPermission(Onyx.Oms.Core.Domain.Constants.Permissions.Tenants.Edit);
    }
}
