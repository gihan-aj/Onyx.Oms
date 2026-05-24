using MediatR;
using Microsoft.AspNetCore.Mvc;
using Onyx.Oms.Core.Domain.Constants;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.Settings.WhatsApp.UpdateWhatsAppSettings
{
    public class GetWhatsAppSettingsEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("api/v{version:apiVersion}/settings/whatsapp")
            .WithApiVersionSet(app.NewApiVersionSet("TenantProfile").Build())
            .HasApiVersion(1);

            group.MapPut("", async (ISender sender, [FromBody] UpdateWhatsAppSettingsCommand command) =>
            {
                var result = await sender.Send(command);
                return result.ToMinimalApiResult();
            })
                .WithTags("Settings")
                .WithSummary("Update WhatsApp settings")
                .WithDescription("Update WhatsApp phone number ID and and Access Token.")
                .Produces(StatusCodes.Status204NoContent)
                .RequireAuthorization()
                .HasPermission(Permissions.Tenants.Edit);
        }
    }
}
