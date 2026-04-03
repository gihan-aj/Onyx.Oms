using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Settings.TenantProfile.UpdateLogo
{
    public record UpdateTenantLogoCommand(string LogoUrl) : ICommand;
}
