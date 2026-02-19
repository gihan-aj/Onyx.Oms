using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Roles.ActivateRole;

public record ActivateRoleCommand(Guid Id) : ICommand;
