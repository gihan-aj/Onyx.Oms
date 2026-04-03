using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Domain.Constants;
using System.Security.Claims;

namespace Onyx.Oms.Infrastructure.Identity;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IServiceProvider _serviceProvider;
    //private readonly IPermissionService _permissionService;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor, IServiceProvider serviceProvider)
    {
        _httpContextAccessor = httpContextAccessor;
        _serviceProvider = serviceProvider;
    }

    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    public Guid UserId
    {
        get
        {
            var userIdString = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? _httpContextAccessor.HttpContext?.User?.FindFirstValue(CustomClaims.Subject);

            return Guid.TryParse(userIdString, out var userId) ? userId : Guid.Empty;
        }
    }

    public Guid ActualTenantId
    {
        get
        {
            var tenantIdString = _httpContextAccessor.HttpContext?.User?.FindFirstValue(CustomClaims.TenantId);

            return Guid.TryParse(tenantIdString, out var tenantId) ? tenantId : Guid.Empty;
        }
    }

    public Guid ActiveTenantId
    {
        get
        {
            if(!IsAuthenticated)
                return Guid.Empty;

            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
                return ActualTenantId;

            // Check if the frontend sent the impersonation header
            if(httpContext.Request.Headers.TryGetValue("X-Tenant-ID", out var requestedTenantString))
            {
                if(Guid.TryParse(requestedTenantString, out var requestedTenantId))
                {
                    using var scope = _serviceProvider.CreateScope();
                    var permissionService = scope.ServiceProvider.GetRequiredService<IPermissionService>();
                    var userPermissions = permissionService.GetPermissionsAsync(UserId).GetAwaiter().GetResult();
                    if (userPermissions != null && userPermissions.Contains(Permissions.Platform.ImpersonateTenant))
                    {
                        return requestedTenantId;
                    }
                }
            }

            return ActualTenantId;
        }
    }

    public bool IsImpersonating => ActualTenantId != ActiveTenantId;
}
