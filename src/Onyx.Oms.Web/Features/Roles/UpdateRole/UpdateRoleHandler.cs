using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Messaging;
using Onyx.Oms.Infrastructure.Identity.IdP;
using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Domain.Models;

namespace Onyx.Oms.Web.Features.Roles.UpdateRole;

public class UpdateRoleHandler : ICommandHandler<UpdateRoleCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public UpdateRoleHandler(IApplicationDbContext context,ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (role == null)
        {
            return Result.Failure(Error.NotFound("Role.NotFound", $"Role with Id {request.Id} was not found."));
        }

        // Protect Admin roles
        if (role.Name == Core.Domain.Constants.Roles.Oms.SystemAdmin || role.Name == Core.Domain.Constants.Roles.Oms.TenantOwner)
        {
            return Result.Failure(Error.Forbidden("Role.Protected", "Modifying this role is not allowed."));
        }

        // Prevent users from modifying their own roles
        var currentUserId = _currentUserService.UserId;

        var isCurrentUserRole = await _context.AppUsers
            .Where(u => u.Id == currentUserId)
            .SelectMany(u => u.Roles)
            .AnyAsync(r => r.Id == request.Id, cancellationToken);

        if (isCurrentUserRole)
        {
            return Result.Failure(Error.Forbidden("Role.ModifyOwn", "You cannot modify a role that is currently assigned to you."));
        }

        // Update Local Details
        role.Update(request.Name, request.Description);

        // Update Permissions
        // 1. Add new permissions
        foreach (var perm in request.Permissions)
        {
            role.AddPermission(perm);
        }

        // 2. Remove permissions not in the list
        var currentPermissions = role.Permissions.ToList();
        foreach (var perm in currentPermissions)
        {
            if (!request.Permissions.Contains(perm))
            {
                role.RemovePermission(perm);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
