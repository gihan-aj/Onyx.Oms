using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Settings.TenantProfile.UpdateRegionalSettings;

public record UpdateRegionalSettingsCommand(
    string DefaultCurrency,
    string TimeZone,
    string WeightUnit
) : ICommand;
