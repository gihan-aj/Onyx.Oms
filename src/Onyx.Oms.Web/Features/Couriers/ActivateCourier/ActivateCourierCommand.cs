using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Couriers.ActivateCourier;

public record ActivateCourierCommand(Guid Id) : ICommand;
