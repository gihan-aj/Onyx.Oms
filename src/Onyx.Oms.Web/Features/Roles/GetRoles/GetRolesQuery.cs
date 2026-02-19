using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Roles.GetRoles;

public record GetRolesQuery(bool? IsActive = null) : IQuery<IEnumerable<RoleDto>>;
