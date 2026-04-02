using Microsoft.Extensions.Caching.Memory;
using Onyx.Oms.Core.Common.Interfaces;

namespace Onyx.Oms.Infrastructure.Identity;

public class CachedPermissionService : IPermissionService
{
    private readonly IPermissionService _permissionService;
    private readonly IMemoryCache _memoryCache;

    public CachedPermissionService(IPermissionService permissionService, IMemoryCache memoryCache)
    {
        _permissionService = permissionService;
        _memoryCache = memoryCache;
    }

    public async Task<HashSet<string>> GetPermissionsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        string key = $"permissions-{userId}";
        
        return await _memoryCache.GetOrCreateAsync(key, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15);
            return await _permissionService.GetPermissionsAsync(userId, cancellationToken);
        }) ?? new HashSet<string>();
    }
}
