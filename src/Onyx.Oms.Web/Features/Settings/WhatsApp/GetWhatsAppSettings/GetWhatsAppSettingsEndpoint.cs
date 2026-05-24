using MediatR;
using Onyx.Oms.Core.Domain.Constants;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.Settings.WhatsApp.GetWhatsAppSettings
{
    public class GetWhatsAppSettingsEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("api/v{version:apiVersion}/settings/whatsapp")
            .WithApiVersionSet(app.NewApiVersionSet("TenantProfile").Build())
            .HasApiVersion(1);

            group.MapGet("", async (ISender sender) =>
            {
                var result = await sender.Send(new GetWhatsAppSettingsQuery());
                return result.ToMinimalApiResult();
            })
                .WithTags("Settings")
                .WithSummary("Get WhatsApp settings")
                .WithDescription("Retrieves WhatsApp phone number ID and and token status.")
                .Produces<WhatsAppSettingsDto>(StatusCodes.Status200OK)
                .RequireAuthorization()
                .HasPermission(Permissions.Tenants.View);
        }
    }
}
