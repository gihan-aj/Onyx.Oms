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

    private const string ActiveTenantContextKey = "ResolvedActiveTenantId";

    public CurrentUserService(IHttpContextAccessor httpContextAccessor, IServiceProvider serviceProvider)
    {
        _httpContextAccessor = httpContextAccessor;
        _serviceProvider = serviceProvider;
    }

    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    public Guid? UserId
    {
        get
        {
            var userIdString = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? _httpContextAccessor.HttpContext?.User?.FindFirstValue(CustomClaims.Subject);

            return Guid.TryParse(userIdString, out var userId) ? userId : null;
        }
    }

    public Guid? ActualTenantId
    {
        get
        {
            var tenantIdString = _httpContextAccessor.HttpContext?.User?.FindFirstValue(CustomClaims.TenantId);

            return Guid.TryParse(tenantIdString, out var tenantId) ? tenantId : null;
        }
    }

    public Guid? ActiveTenantId
    {
        get
        {
            var httpContext = _httpContextAccessor.HttpContext;
            // Read from the Middleware Cache
            if (httpContext != null && httpContext.Items.TryGetValue(ActiveTenantContextKey, out var cachedTenantId))
            {
                return (Guid?)cachedTenantId;
            }

            // Safe fallback if there is no HTTP Context (e.g., background jobs)
            return ActualTenantId;
        }
    }

    public bool IsImpersonating =>
        ActualTenantId.HasValue &&
        ActiveTenantId.HasValue &&
        ActualTenantId.Value != ActiveTenantId.Value;

    public async Task<Guid?> GetActiveTenantIdAsync(CancellationToken cancellationToken = default)
    {
        if (!IsAuthenticated)
            return null;

        var httpContext = _httpContextAccessor?.HttpContext;
        if (httpContext == null)
            return ActualTenantId;

        if (httpContext.Items.TryGetValue(ActiveTenantContextKey, out var cachedTenantId))
        {
            return (Guid?)cachedTenantId;
        }

        var actualTenantId = ActualTenantId;

        if (httpContext.Request.Headers.TryGetValue("X-Tenant-ID", out var requestedTenantString) &&
            Guid.TryParse(requestedTenantString, out var requestedTenantId))
        {
            var currentUserId = UserId;

            if (currentUserId.HasValue)
            {
                await using var scope = _serviceProvider.CreateAsyncScope();
                var permissionService = scope.ServiceProvider.GetRequiredService<IPermissionService>();

                var userPermissions = await permissionService.GetPermissionsAsync(currentUserId.Value, null, cancellationToken);

                if (userPermissions != null && userPermissions.Contains(Permissions.Platform.ImpersonateTenant))
                {
                    // Cache and return the impersonated tenant
                    httpContext.Items[ActiveTenantContextKey] = requestedTenantId;
                    return requestedTenantId;
                }
            }
        }

        // Cache and return the actual tenant if impersonation failed or wasn't requested
        httpContext.Items[ActiveTenantContextKey] = actualTenantId;
        return actualTenantId;
    }
}
