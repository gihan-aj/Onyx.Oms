using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Couriers.UpdateCourier;

public record UpdateCourierCommand(
    Guid Id,
    string Name,
    string? ContactPerson,
    string? PrimaryPhone,
    string? SecondaryPhone,
    string? WebsiteUrl,
    string? TrackingUrlTemplate) : ICommand;
