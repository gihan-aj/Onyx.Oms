using MediatR;
using Microsoft.AspNetCore.Mvc;
using Onyx.Oms.Core.Domain.Constants;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.Roles.CreateRole;

public class CreateRoleEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/v{version:apiVersion}/roles")
            .WithApiVersionSet(app.NewApiVersionSet("Roles").Build()) 
            .HasApiVersion(1);

        group.MapPost("", async (ISender sender, [FromBody] CreateRoleCommand command) =>
        {
            var result = await sender.Send(command);
            return result.ToMinimalApiResult();
        })
        .WithTags("Roles")
        .WithName("CreateRole")
        .WithSummary("Create a new role")
        .WithDescription("Creates a role locally and synchronizes it with the Identity Provider.")
        .HasPermission(Permissions.Roles.Create);
    }
}
