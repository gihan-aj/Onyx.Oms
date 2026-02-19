using Asp.Versioning;
using MediatR;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Constants;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.Roles.GetRolesPaged;

public class GetRolesPagedEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/v{version:apiVersion}/roles")
            .WithApiVersionSet(app.NewApiVersionSet("Roles").Build())
            .HasApiVersion(1);

        group.MapGet("search", async (ISender sender, [AsParameters] GetRolesPagedQuery query) =>
        {
            var result = await sender.Send(query);
            return result.ToMinimalApiResult();
        })
        .WithTags("Roles")
        .WithName("GetRolesPaged")
        .WithSummary("Get roles (Paged)")
        .WithDescription("Retrieves a paginated list of roles with optional searching and sorting.")
        .HasPermission(Permissions.Roles.View);
    }
}
