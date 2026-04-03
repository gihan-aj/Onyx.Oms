using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Entities;
using Onyx.Oms.Core.Messaging;
using Onyx.Oms.Infrastructure.Identity.IdP;
using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Domain.Models;

namespace Onyx.Oms.Web.Features.Roles.CreateRole;

public class CreateRoleHandler : ICommandHandler<CreateRoleCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public CreateRoleHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<Guid>> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        // 1. Check if Role exists locally
        if (await _context.Roles.AnyAsync(r => r.Name == request.Name, cancellationToken))
        {
            return Result.Failure<Guid>(Error.Conflict("Role.Exists", $"Role '{request.Name}' already exists."));
        }

        // 4. Create Local Role
        var roleResult = Role.Create(_currentUserService.ActiveTenantId, request.Name, request.Description);
        if(roleResult.IsFailure)
            return Result.Failure<Guid>(roleResult.Error);

        var role = roleResult.Value;

        // 5. Add Permissions
        foreach (var perm in request.Permissions)
        {
             role.AddPermission(perm);
        }

        _context.Roles.Add(role);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(role.Id);
    }
}
