using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Settings.TenantProfile.UpdatePreferences;

public record UpdatePreferencesCommand(
    string PreferencesJson
) : ICommand;
