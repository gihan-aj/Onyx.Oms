using MediatR;
using Onyx.Oms.Core.Common.Models;

namespace Onyx.Oms.Web.Features.Roles.CreateRole;

public record CreateRoleCommand(
    string Name,
    string? Description,
    List<string> Permissions) : IRequest<Result<Guid>>;
