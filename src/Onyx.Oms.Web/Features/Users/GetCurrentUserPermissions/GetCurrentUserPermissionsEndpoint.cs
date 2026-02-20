using Asp.Versioning;
using MediatR;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.Users.GetCurrentUserPermissions;

public class GetCurrentUserPermissionsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/v{version:apiVersion}/users/me")
            .WithApiVersionSet(app.NewApiVersionSet("Users").Build())
            .HasApiVersion(1);

        group.MapGet("permissions", async (ISender sender, [AsParameters] GetCurrentUserPermissionsQuery query) =>
        {
            var result = await sender.Send(query);
            return result.ToMinimalApiResult();
        })
        .WithTags("Users")
        .WithName("GetCurrentUserPermissions")
        .WithSummary("Get current user permissions")
        .WithDescription("Returns a flattened list of all permissions the currently logged-in user possesses.")
        .RequireAuthorization(); // Requires being logged in
    }
}
