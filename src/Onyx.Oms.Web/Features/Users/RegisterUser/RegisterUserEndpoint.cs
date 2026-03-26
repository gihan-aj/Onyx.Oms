using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.Users.RegisterUser;

public class RegisterUserEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/v{version:apiVersion}/users")
            .WithApiVersionSet(app.NewApiVersionSet("Users").Build())
            .HasApiVersion(1);

        group.MapPost("register", async (ISender sender, [FromBody] RegisterUserCommand command) =>
        {
            var result = await sender.Send(command);
            return result.ToMinimalApiResult();
        })
        .WithTags("Users")
        .WithName("RegisterUser")
        .WithSummary("Register a new tenant and user")
        .WithDescription("Creates a new tenant, subscription, and registers the initial admin user in the system.")
        .AllowAnonymous();
    }
}
