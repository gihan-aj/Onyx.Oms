using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Couriers.CreateCourier;

public record CreateCourierZoneRateDto(
    string ZoneName,
    decimal BaseFee,
    decimal BaseWeight,
    decimal ExcessFeePerWeightUnit,
    decimal CodPercentage,
    string Currency,
    string WeightUnit,
    bool IsDefault,
    List<string> CoveredDistricts);

public record CreateCourierCommand(
    string Name,
    string? ContactPerson,
    string? PrimaryPhone,
    string? SecondaryPhone,
    string? WebsiteUrl,
    string? TrackingUrlTemplate,
    List<CreateCourierZoneRateDto>? ZoneRates) : ICommand<Guid>;
