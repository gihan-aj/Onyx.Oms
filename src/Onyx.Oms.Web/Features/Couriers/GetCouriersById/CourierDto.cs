namespace Onyx.Oms.Web.Features.Couriers.GetCouriersById;

public record CourierDto(
    Guid Id,
    string Name,
    string? ContactPerson,
    string? PrimaryPhone,
    string? SecondaryPhone,
    string? WebsiteUrl,
    string? TrackingUrlTemplate,
    bool IsActive);
