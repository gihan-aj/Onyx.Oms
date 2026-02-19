using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Roles.GetRolesPaged;

public record GetRolesPagedQuery : PagedRequest, IQuery<PagedResult<RoleDto>>
{
    public bool? IsActive { get; init; }
}

public record RoleDto(Guid Id, string Name, string? Description, int PermissionCount, int UserCount, bool IsActive);
