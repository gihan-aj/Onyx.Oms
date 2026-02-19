using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Roles.UpdateRole;

public record UpdateRoleCommand(
    Guid Id,
    string Name,
    string? Description,
    List<string> Permissions) : ICommand;
