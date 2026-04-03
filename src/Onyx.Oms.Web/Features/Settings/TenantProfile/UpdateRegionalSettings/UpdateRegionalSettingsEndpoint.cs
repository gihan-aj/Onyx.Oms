using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.Settings.TenantProfile.UpdateRegionalSettings;

public class UpdateRegionalSettingsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/v{version:apiVersion}/settings/profile")
            .WithApiVersionSet(app.NewApiVersionSet("TenantProfile").Build()) 
            .HasApiVersion(1);

        group.MapPut("regional-settings", async ([FromBody] UpdateRegionalSettingsCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.ToMinimalApiResult();
        })
        .WithTags("Settings")
        .WithName("UpdateRegionalSettings")
        .WithSummary("Update regional settings")
        .WithDescription("Updates technical business settings such as weight units and currency.")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .RequireAuthorization()
        .HasPermission(Onyx.Oms.Core.Domain.Constants.Permissions.Tenants.Edit);
    }
}
