using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.Settings.TenantProfile.UpdateHeroImage
{
    public class UpdateTenantHeroImageEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("api/v{version:apiVersion}/settings/profile")
                .WithApiVersionSet(app.NewApiVersionSet("TenantProfile").Build())
                .HasApiVersion(1);

            group.MapPut("hero-image", async ([FromBody] UpdateTenantHeroImageCommand command, ISender sender) =>
            {
                var result = await sender.Send(command);
                return result.ToMinimalApiResult();
            })
                .WithTags("Settings")
                .WithName("UpdateHeroImage")
                .WithSummary("Update Hero Image URL")
                .WithDescription("Updates tenant Hero Image url.")
                .Produces(StatusCodes.Status204NoContent)
                .RequireAuthorization()
                .HasPermission(Onyx.Oms.Core.Domain.Constants.Permissions.Tenants.Edit);
        }
    }
}
