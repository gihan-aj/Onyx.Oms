using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Onyx.Oms.Core.Common.Interfaces;

namespace Onyx.Oms.Infrastructure.Identity;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? UserId => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                             ?? _httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value;

    public string? UserName => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value 
                               ?? _httpContextAccessor.HttpContext?.User?.FindFirst("name")?.Value;

    public string? Email => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value 
                            ?? _httpContextAccessor.HttpContext?.User?.FindFirst("email")?.Value;

    // Looking for 'client_id' (standard) or 'azp' (Authorized Party - common in OIDC for the client)
    public string? ClientId => _httpContextAccessor.HttpContext?.User?.FindFirst("client_id")?.Value 
                               ?? _httpContextAccessor.HttpContext?.User?.FindFirst("azp")?.Value;

    public string? TenantId => _httpContextAccessor.HttpContext?.User?.FindFirst("tenant_id")?.Value;

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
}
