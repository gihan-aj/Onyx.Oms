using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Settings.WhatsApp.GetWhatsAppSettings
{
    public class GetWhatsAppSettingsQuery() : IQuery<WhatsAppSettingsDto>;

    public record WhatsAppSettingsDto(string? PhoneNumberId, bool IsConfigured);
}
