using MediatR;
using Microsoft.AspNetCore.Mvc;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.Settings.TenantProfile.UpdatePreferences;

public class UpdatePreferencesEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/v{version:apiVersion}/settings/profile")
            .WithApiVersionSet(app.NewApiVersionSet("TenantProfile").Build()) 
            .HasApiVersion(1);

        group.MapPut("preferences", async ([FromBody] UpdatePreferencesCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.ToMinimalApiResult();
        })
        .WithTags("Settings")
        .WithName("UpdatePreferences")
        .WithSummary("Update UI preferences")
        .WithDescription("Updates the JSON structured user interface preferences for the store.")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .RequireAuthorization()
        .HasPermission(Onyx.Oms.Core.Domain.Constants.Permissions.Tenants.Edit);
    }
}
