using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Couriers.GetCouriersPaged;

public record GetCouriersPagedQuery : PagedRequest, IQuery<PagedResult<CourierDto>>
{
    public bool? IsActive { get; init; } // Optional filter specific to Couriers
}

public record CourierDto(
    Guid Id,
    string Name,
    string? ContactPerson,
    string? PrimaryPhone,
    string? SecondaryPhone,
    string? WebsiteUrl,
    bool IsActive);
