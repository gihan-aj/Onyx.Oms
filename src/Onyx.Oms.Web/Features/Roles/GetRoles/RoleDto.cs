namespace Onyx.Oms.Web.Features.Roles.GetRoles;

public record RoleDto(Guid Id, string Name, string? Description, int PermissionCount, bool IsActive);
