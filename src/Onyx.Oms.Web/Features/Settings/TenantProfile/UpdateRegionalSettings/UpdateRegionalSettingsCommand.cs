using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Settings.TenantProfile.UpdateRegionalSettings;

public record UpdateRegionalSettingsCommand(
    string BaseCurrency,
    string WeightUnit
) : ICommand;
