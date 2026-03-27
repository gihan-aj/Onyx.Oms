using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Messaging;
using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Domain.Models;

namespace Onyx.Oms.Web.Features.Roles.DeleteRole;

public class DeleteRoleHandler : ICommandHandler<DeleteRoleCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public DeleteRoleHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);
        if (role == null)
        {
            return Result.Failure(Error.NotFound("Role.NotFound", $"Role with Id {request.Id} was not found."));
        }

        // Protect SuperAdmin role
        if (role.Name == "SuperAdmin")
        {
            return Result.Failure(Error.Forbidden("Role.Protected", "Modifying the SuperAdmin role is not allowed."));
        }

        // Prevent users from modifying their own roles
        var currentUserIdString = _currentUserService.UserId;
        var currentUserId = Guid.Empty;
        if (string.IsNullOrEmpty(currentUserIdString))
        {
            return Result.Failure(Error.Unauthorized("DeleteRole.NotAuthenticated", "User is not authenticated."));
        }
        if (!Guid.TryParse(currentUserIdString, out currentUserId))
        {
            return Result.Failure(Error.Unauthorized("DeleteRole.NotAuthenticated", "User is not authenticated."));
        }
        var isCurrentUserRole = await _context.AppUsers
            .Where(u => u.Id == currentUserId)
            .SelectMany(u => u.Roles)
            .AnyAsync(r => r.Id == request.Id, cancellationToken);

        if (isCurrentUserRole)
        {
            return Result.Failure(Error.Forbidden("Role.ModifyOwn", "You cannot modify a role that is currently assigned to you."));
        }

        _context.Roles.Remove(role);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
