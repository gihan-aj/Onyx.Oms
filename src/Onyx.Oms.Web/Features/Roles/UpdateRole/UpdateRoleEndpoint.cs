using MediatR;
using Microsoft.AspNetCore.Mvc;
using Onyx.Oms.Core.Domain.Constants;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.Roles.UpdateRole;

public class UpdateRoleEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/v{version:apiVersion}/roles")
            .WithApiVersionSet(app.NewApiVersionSet("Roles").Build())
            .HasApiVersion(1);

        group.MapPut("{id:guid}", async (ISender sender, Guid id, [FromBody] UpdateRoleCommand command) =>
        {
            // Note: Since records are immutable by default regarding properties in primary constructor unless init-only,
            // matching ID logic is usually good practice. 
            // In C# records, we might need a trusted way to ensure Id matches.
            // Here we assume client sends correct payload or we can double check.
            if (id != command.Id) return Results.BadRequest("Id mismatch");

            var result = await sender.Send(command);
            return result.ToMinimalApiResult();
        })
        .WithTags("Roles")
        .WithName("UpdateRole")
        .WithSummary("Update a role")
        .WithDescription("Updates the role name, description, and permissions. Renaming syncs with IdP.")
        .HasPermission(Permissions.Roles.Edit);
    }
}
