using Onyx.Oms.Core.Domain.ValueObjects;

namespace Onyx.Oms.Web.Features.Settings.TenantProfile.GetProfile;

public record TenantProfileDto(
    Guid Id,
    string StoreName,
    string? LegalName,
    string? TaxRegistrationNumber,
    string ContactEmail,
    string? ContactPhone,
    Address? StoreAddress,
    string BaseCurrency,
    string WeightUnit,
    string? InvoiceFooterText,
    string? LogoUrl,
    string PreferencesJson
);
