using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Onyx.Oms.Core.Domain.Constants;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.Roles.GetRoles;

public class GetRolesEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/v{version:apiVersion}/roles")
            .WithApiVersionSet(app.NewApiVersionSet("Roles").Build())
            .HasApiVersion(1);

        group.MapGet("", async (ISender sender, [AsParameters] GetRolesQuery query) =>
        {
            var result = await sender.Send(query);
            return result.ToMinimalApiResult();
        })
        .WithTags("Roles")
        .WithName("GetRoles")
        .WithSummary("Get all roles")
        .WithDescription("Retrieves a paginated list of roles.")
        .HasPermission(Permissions.Roles.View);
    }
}
