namespace Onyx.Oms.Web.Features.Couriers.GetCouriersById;

public record CourierZoneRateDto(
    Guid Id,
    string ZoneName,
    decimal BaseFee,
    string BaseFeeCurrency,
    decimal BaseWeight,
    string BaseWeightUnit,
    decimal ExcessFeePerWeightUnit,
    string ExcessFeePerWeightUnitCurrency,
    decimal CodPercentage,
    bool IsDefault,
    IReadOnlyCollection<string> CoveredDistricts);

public record CourierDto(
    Guid Id,
    string Name,
    string? ContactPerson,
    string? PrimaryPhone,
    string? SecondaryPhone,
    string? WebsiteUrl,
    string? TrackingUrlTemplate,
    bool IsActive,
    IReadOnlyCollection<CourierZoneRateDto> ZoneRates);
