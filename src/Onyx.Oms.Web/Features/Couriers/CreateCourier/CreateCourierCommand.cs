using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Couriers.CreateCourier;

public record CreateCourierCommand(
    string Name,
    string? ContactPerson,
    string? PrimaryPhone,
    string? SecondaryPhone,
    string? WebsiteUrl,
    string? TrackingUrlTemplate) : ICommand<Guid>;
