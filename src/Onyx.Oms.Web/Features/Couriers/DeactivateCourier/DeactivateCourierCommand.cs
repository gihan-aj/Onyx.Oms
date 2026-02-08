using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Couriers.DeactivateCourier;

public record DeactivateCourierCommand(Guid Id) : ICommand;
