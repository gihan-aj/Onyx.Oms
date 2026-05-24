using Onyx.Oms.Core.Messaging;
using Onyx.Oms.Web.Features.Settings.WhatsApp.GetWhatsAppSettings;

namespace Onyx.Oms.Web.Features.Settings.WhatsApp.UpdateWhatsAppSettings
{
    public record UpdateWhatsAppSettingsCommand(string PhoneNumberId, string? AccessToken) : ICommand;
}
