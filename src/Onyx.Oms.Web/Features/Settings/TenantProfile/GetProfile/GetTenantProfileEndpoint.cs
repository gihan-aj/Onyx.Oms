using MediatR;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.Settings.TenantProfile.GetProfile;

public class GetTenantProfileEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/v{version:apiVersion}/settings/profile")
            .WithApiVersionSet(app.NewApiVersionSet("TenantProfile").Build()) 
            .HasApiVersion(1);

        group.MapGet("", async (ISender sender) =>
        {
            var result = await sender.Send(new GetTenantProfileQuery());
            return result.ToMinimalApiResult();
        })
        .WithTags("Settings")
        .WithSummary("Get tenant profile")
        .WithDescription("Retrieves the global tenant profile containing store settings.")
        .Produces<TenantProfileDto>(StatusCodes.Status200OK)
        .RequireAuthorization()
        .HasPermission(Onyx.Oms.Core.Domain.Constants.Permissions.Tenants.View);
    }
}
