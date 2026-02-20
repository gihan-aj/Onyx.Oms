using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Roles.GetPermissions;

public record GetPermissionsQuery() : IQuery<List<PermissionGroupDto>>;

public record PermissionGroupDto(string GroupName, List<PermissionDto> Permissions);

public record PermissionDto(string Name, string Value);
