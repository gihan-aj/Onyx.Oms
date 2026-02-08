namespace Onyx.Oms.Web.Features.Couriers.GetCouriers;

public record CourierDto(
    Guid Id,
    string Name,
    string? ContactPerson,
    string? PrimaryPhone,
    string? SecondaryPhone,
    string? WebsiteUrl,
    bool IsActive);
