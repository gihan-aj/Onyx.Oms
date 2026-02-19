using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Roles.DeleteRole;

public record DeleteRoleCommand(Guid Id) : ICommand;
