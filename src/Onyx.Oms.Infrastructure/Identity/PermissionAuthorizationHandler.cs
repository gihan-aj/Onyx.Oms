using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Onyx.Oms.Core.Common.Interfaces;

namespace Onyx.Oms.Infrastructure.Identity;

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IPermissionService _permissionService;
    private readonly ILogger<PermissionAuthorizationHandler> _logger;
    private readonly IApplicationDbContext _context;

    public PermissionAuthorizationHandler(
        ICurrentUserService currentUserService,
        IPermissionService permissionService,
        ILogger<PermissionAuthorizationHandler> logger,
        IApplicationDbContext context) // Inject context to lookup int ID
    {
        _currentUserService = currentUserService;
        _permissionService = permissionService;
        _logger = logger;
        _context = context;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            return;
        }

        var sub = _currentUserService.UserId;
        var userId = Guid.Empty;
        if (string.IsNullOrEmpty(sub) && Guid.TryParse(sub, out userId))
        {
            _logger.LogWarning("Permission check failed: User authenticated but no ID found.");
            return;
        }

        // NOTE: In a real high-scale app, we might cache this mapping too or put the int ID in the token claims
        var user = await _context.AppUsers
            .Where(u => u.Id == userId)
            .FirstOrDefaultAsync();
        
        if (user == null)
        {
             _logger.LogWarning("Permission check failed: User {Sub} not found in local DB.", sub);
             return;
        }

        var permissions = await _permissionService.GetPermissionsAsync(user.Id);

        if (permissions is not null && permissions.Contains(requirement.Permission))
        {
            context.Succeed(requirement);
        }
        else
        {
             _logger.LogInformation("Permission denied. User {UserId} requires {Permission}.", user.Id, requirement.Permission);
        }
    }
}
