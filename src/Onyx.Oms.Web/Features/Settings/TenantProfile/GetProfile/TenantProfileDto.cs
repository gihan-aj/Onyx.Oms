using Onyx.Oms.Core.Domain.ValueObjects;

namespace Onyx.Oms.Web.Features.Settings.TenantProfile.GetProfile;

public record TenantProfileDto(
    Guid Id,
    string StoreName,
    string ContactEmail,
    string? ContactPhone,
    string? LegalName,
    string? TaxRegistrationNumber,
    Address? StoreAddress,
    string DefaultCurrency,
    string TimeZone,
    string WeightUnit,
    string? InvoiceFooterText,
    string? LogoUrl,
    string? HeroImageUrl,
    string PreferencesJson
);
