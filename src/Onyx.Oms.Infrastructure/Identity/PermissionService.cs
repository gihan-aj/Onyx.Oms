using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;

namespace Onyx.Oms.Infrastructure.Identity;

public class PermissionService : IPermissionService
{
    private readonly IApplicationDbContext _context;

    public PermissionService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<HashSet<string>?> GetPermissionsAsync(int userId)
    {
        // 1. Get the user's roles
        var user = await _context.AppUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null || !user.Roles.Any())
        {
            return new HashSet<string>();
        }

        // 2. Get the permissions for those roles
        var roles = await _context.Roles
            .AsNoTracking()
            .Where(r => user.Roles.Contains(r.Name))
            .ToListAsync();

        // 3. Flatten into a distinct set of permissions
        var permissions = roles
            .SelectMany(r => r.Permissions)
            .ToHashSet();

        return permissions;
    }
}
