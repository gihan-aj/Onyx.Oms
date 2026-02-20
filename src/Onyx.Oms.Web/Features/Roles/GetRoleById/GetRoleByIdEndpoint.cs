using Asp.Versioning;
using MediatR;
using Onyx.Oms.Core.Domain.Constants;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.Roles.GetRoleById;

public class GetRoleByIdEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/v{version:apiVersion}/roles")
            .WithApiVersionSet(app.NewApiVersionSet("Roles").Build())
            .HasApiVersion(1);

        group.MapGet("{id:guid}", async (ISender sender, Guid id) =>
        {
            var result = await sender.Send(new GetRoleByIdQuery(id));
            return result.ToMinimalApiResult();
        })
        .WithTags("Roles")
        .WithName("GetRoleById")
        .WithSummary("Get a role by ID")
        .WithDescription("Retrieves the details of a specific role, including its assigned permissions.")
        .HasPermission(Permissions.Roles.View);
    }
}
