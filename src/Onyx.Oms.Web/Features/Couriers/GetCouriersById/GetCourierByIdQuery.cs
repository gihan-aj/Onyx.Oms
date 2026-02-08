using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Couriers.GetCouriersById;

public record GetCourierByIdQuery(Guid Id) : IQuery<CourierDto>;
