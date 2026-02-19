using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Onyx.Oms.Core.Domain.Constants;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.Users.InviteUser;

public class InviteUserEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/v{version:apiVersion}/users")
            .WithApiVersionSet(app.NewApiVersionSet("Users").Build())
            .HasApiVersion(1);

        group.MapPost("invite", async (ISender sender, [FromBody] InviteUserCommand command) =>
        {
            var result = await sender.Send(command);
            return result.ToMinimalApiResult();
        })
        .WithTags("Users")
        .WithName("InviteUser")
        .WithSummary("Invite a user")
        .WithDescription("Invites a user via the Identity Provider and assigns them a role.")
        .HasPermission(Permissions.Users.Create);
    }
}
