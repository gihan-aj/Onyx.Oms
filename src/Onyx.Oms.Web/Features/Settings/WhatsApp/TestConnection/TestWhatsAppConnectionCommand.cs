using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Settings.WhatsApp.TestConnection
{
    public record TestWhatsAppConnectionCommand(string DestinationPhone) : ICommand<string>;
}
