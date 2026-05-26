using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Couriers.UpdateCourier;

public record UpdateCourierZoneRateDto(
    Guid? Id,
    string ZoneName,
    decimal BaseFee,
    decimal BaseWeight,
    decimal ExcessFeePerWeightUnit,
    decimal CodPercentage,
    string Currency,
    string WeightUnit,
    bool IsDefault,
    List<string> CoveredDistricts);

public record UpdateCourierCommand(
    Guid Id,
    string Name,
    string? ContactPerson,
    string? PrimaryPhone,
    string? SecondaryPhone,
    string? WebsiteUrl,
    string? TrackingUrlTemplate,
    List<UpdateCourierZoneRateDto>? ZoneRates) : ICommand;
