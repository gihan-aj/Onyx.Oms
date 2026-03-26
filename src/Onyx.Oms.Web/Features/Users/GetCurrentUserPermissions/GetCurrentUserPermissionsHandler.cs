using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Messaging;
using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Domain.Models;

namespace Onyx.Oms.Web.Features.Users.GetCurrentUserPermissions;

public class GetCurrentUserPermissionsHandler : IQueryHandler<GetCurrentUserPermissionsQuery, List<string>>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IPermissionService _permissionService;
    private readonly IApplicationDbContext _context;

    public GetCurrentUserPermissionsHandler(
        ICurrentUserService currentUserService,
        IPermissionService permissionService,
        IApplicationDbContext context)
    {
        _currentUserService = currentUserService;
        _permissionService = permissionService;
        _context = context;
    }

    public async Task<Result<List<string>>> Handle(GetCurrentUserPermissionsQuery request, CancellationToken cancellationToken)
    {
        var sub = _currentUserService.UserId;
        var userId = Guid.Empty;
        if (string.IsNullOrEmpty(sub))
        {
            return Result.Failure<List<string>>(Error.Unauthorized("Identity.NotAuthenticated", "User is not authenticated."));
        }
        if (!Guid.TryParse(sub, out userId))
        {
            return Result.Failure<List<string>>(Error.Unauthorized("Identity.NotAuthenticated", "User is not authenticated."));
        }

        // We need the local user's Guid to get permissions
        var localUser = await _context.AppUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (localUser == null)
        {
             return Result.Failure<List<string>>(Error.NotFound("User.NotFound", "Local user profile not found."));
        }

        var permissions = await _permissionService.GetPermissionsAsync(localUser.Id);

        return Result.Success(permissions?.ToList() ?? new List<string>());
    }
}
