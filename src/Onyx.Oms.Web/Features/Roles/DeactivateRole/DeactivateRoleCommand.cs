using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Roles.DeactivateRole;

public record DeactivateRoleCommand(Guid Id) : ICommand;
