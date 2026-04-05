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

    public PermissionAuthorizationHandler(
        ICurrentUserService currentUserService,
        IPermissionService permissionService,
        ILogger<PermissionAuthorizationHandler> logger)
    {
        _currentUserService = currentUserService;
        _permissionService = permissionService;
        _logger = logger;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        var userId = _currentUserService.UserId;
        if (!_currentUserService.IsAuthenticated || userId == null)
        {
            return;
        }

        var userPermissions = await _permissionService.GetPermissionsAsync(userId.Value, _currentUserService.ActiveTenantId);

        if (userPermissions.Contains(requirement.Permission))
        {
            context.Succeed(requirement);
        }
        else
        {
            _logger.LogInformation("Permission denied. User {UserId} requires {Permission}.", _currentUserService.UserId, requirement.Permission);
        }
    }
}
