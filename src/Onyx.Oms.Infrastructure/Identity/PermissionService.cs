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

    public async Task<HashSet<string>?> GetPermissionsAsync(Guid userId)
    {
        // 1. Get the user's roles
        var user = await _context.AppUsers
            .AsNoTracking()
            .Include(u => u.Roles) // Include roles from M2M table
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null || !user.Roles.Any())
        {
            return new HashSet<string>();
        }

        // 2. Flatten into a distinct set of permissions from all roles
        var permissions = user.Roles
            .Where(r => r.IsActive)
            .SelectMany(r => r.Permissions)
            .ToHashSet();

        return permissions;
    }
}
