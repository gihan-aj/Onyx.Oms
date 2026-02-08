using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Couriers.DeleteCourier;

public record DeleteCourierCommand(Guid Id) : ICommand;
