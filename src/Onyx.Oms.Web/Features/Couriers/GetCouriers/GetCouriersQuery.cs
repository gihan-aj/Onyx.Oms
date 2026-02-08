using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Couriers.GetCouriers;

public record GetCouriersQuery(bool? IsActive = null) : IQuery<IEnumerable<CourierDto>>;
