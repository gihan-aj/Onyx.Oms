using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Roles.CreateRole;

public record CreateRoleCommand(string Name, string? Description, List<string> Permissions) : ICommand<Guid>;
