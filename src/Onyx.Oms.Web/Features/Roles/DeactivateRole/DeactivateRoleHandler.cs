using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Messaging;
using Onyx.Oms.Infrastructure.Identity.IdP;
using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Domain.Models;

namespace Onyx.Oms.Web.Features.Roles.DeactivateRole;

public class DeactivateRoleHandler : ICommandHandler<DeactivateRoleCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityProviderApi _idpApi;
    private readonly ICurrentUserService _currentUserService;

    public DeactivateRoleHandler(IApplicationDbContext context, IIdentityProviderApi idpApi, ICurrentUserService currentUserService)
    {
        _context = context;
        _idpApi = idpApi;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(DeactivateRoleCommand request, CancellationToken cancellationToken)
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
        var currentUserId = _currentUserService.UserId;
        var isCurrentUserRole = await _context.AppUsers
            .Where(u => u.IdentityUserId == currentUserId)
            .SelectMany(u => u.Roles)
            .AnyAsync(r => r.Id == request.Id, cancellationToken);

        if (isCurrentUserRole)
        {
            return Result.Failure(Error.Forbidden("Role.ModifyOwn", "You cannot modify a role that is currently assigned to you."));
        }

        // Local Only Deactivation
        role.Deactivate();
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
