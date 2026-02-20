using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Roles.GetRoleById;

public record GetRoleByIdQuery(Guid Id) : IQuery<RoleDetailDto>;

public record RoleDetailDto(Guid Id, string Name, string? Description, bool IsActive, List<string> Permissions);
