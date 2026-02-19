using Asp.Versioning;
using MediatR;
using Onyx.Oms.Core.Domain.Constants;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.Roles.DeleteRole;

public class DeleteRoleEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/v{version:apiVersion}/roles")
            .WithApiVersionSet(app.NewApiVersionSet("Roles").Build())
            .HasApiVersion(1);

        group.MapDelete("{id:guid}", async (ISender sender, Guid id) =>
        {
            var result = await sender.Send(new DeleteRoleCommand(id));
            return result.ToMinimalApiResult();
        })
        .WithTags("Roles")
        .WithName("DeleteRole")
        .WithSummary("Delete a role")
        .WithDescription("Deletes a role locally and removes it from the Identity Provider.")
        .HasPermission(Permissions.Roles.Delete);
    }
}
