using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Messaging;
using Onyx.Oms.Web.Features.Couriers.GetCouriers; // Reusing CourierDto

namespace Onyx.Oms.Web.Features.Couriers.GetCouriersPaged;

public record GetCouriersPagedQuery : PagedRequest, IQuery<PagedResult<CourierDto>>
{
    public bool? IsActive { get; init; } // Optional filter specific to Couriers
}
