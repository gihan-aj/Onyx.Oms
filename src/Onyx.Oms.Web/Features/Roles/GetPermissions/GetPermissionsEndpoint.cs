using Asp.Versioning;
using MediatR;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.Roles.GetPermissions;

public class GetPermissionsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        // Notice we MapGroup to "permissions" directly as it's a global concept
        var group = app.MapGroup("api/v{version:apiVersion}/permissions")
            .WithApiVersionSet(app.NewApiVersionSet("Permissions").Build())
            .HasApiVersion(1);

        // We require authenticated users, but maybe not a specific permission to list them
        // usually, if you can View Roles, you can View Permissions.
        group.MapGet("", async (ISender sender, [AsParameters] GetPermissionsQuery query) =>
        {
            var result = await sender.Send(query);
            return result.ToMinimalApiResult();
        })
        .WithTags("Permissions")
        .WithName("GetPermissions")
        .WithSummary("Get all available system permissions")
        .WithDescription("Returns a grouped list of all permissions available in the system.")
        .RequireAuthorization(); // Just requires a valid login token
    }
}
