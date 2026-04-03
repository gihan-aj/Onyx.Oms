using MediatR;
using Microsoft.AspNetCore.Mvc;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.Settings.TenantProfile.UpdateLogo
{
    public class UpdateTenantLogoEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("api/v{version:apiVersion}/settings/profile")
                .WithApiVersionSet(app.NewApiVersionSet("TenantProfile").Build())
                .HasApiVersion(1);

            group.MapPut("logo", async ([FromBody] UpdateTenantLogoCommand command, ISender sender) =>
            {
                var result = await sender.Send(command);
                return result.ToMinimalApiResult();
            })
                .WithTags("Settings")
                .WithName("UpdateLogo")
                .WithSummary("Update logo URL")
                .WithDescription("Updates tenant logo url.")
                .Produces(StatusCodes.Status204NoContent)
                .RequireAuthorization()
                .HasPermission(Onyx.Oms.Core.Domain.Constants.Permissions.Tenants.Edit);
        }
    }
}
