namespace Onyx.Oms.Web.Common.Settings;

public class DefaultTenantProfileSettings
{
    public const string SectionName = "DefaultTenantProfile";

    public string StoreName { get; init; } = string.Empty;
    public string ContactEmail { get; init; } = string.Empty;
    public string BaseCurrency { get; init; } = "LKR";
    public string WeightUnit { get; init; } = "kg";
}
