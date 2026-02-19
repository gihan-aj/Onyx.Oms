using MediatR;
using Onyx.Oms.Core.Domain.Constants;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.Roles.ActivateRole;

public class ActivateRoleEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/v{version:apiVersion}/roles")
            .WithApiVersionSet(app.NewApiVersionSet("Roles").Build())
            .HasApiVersion(1);

        group.MapPut("{id:guid}/activate", async (ISender sender, Guid id) =>
        {
            var result = await sender.Send(new ActivateRoleCommand(id));
            return result.ToMinimalApiResult();
        })
        .WithTags("Roles")
        .WithName("ActivateRole")
        .WithSummary("Activate a role")
        .WithDescription("Activates a role locally and in the Identity Provider.")
        .HasPermission(Permissions.Roles.Activate);
    }
}
