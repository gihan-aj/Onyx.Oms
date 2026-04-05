using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Domain.Constants;

namespace Onyx.Oms.Infrastructure.Identity;

public class PermissionService : IPermissionService
{
    private readonly IApplicationDbContext _context;

    public PermissionService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<HashSet<string>> GetPermissionsAsync(Guid userId, Guid? tenantId = null, CancellationToken cancellationToken = default)
    {
        // Get the user's roles
        var usersQuery = _context.AppUsers
            .IgnoreQueryFilters()
            .AsNoTracking()
            .AsQueryable();

        if(tenantId != null && tenantId != Tenants.HostTenant.Id)
        {
            usersQuery = usersQuery
                .Include(u => u.Roles.Where(r => r.IsActive && (r.TenantId == tenantId || r.TenantId == Tenants.HostTenant.Id)));
        }
        else
        {
            usersQuery = usersQuery
                .Include(u => u.Roles.Where(r => r.IsActive && r.TenantId == Tenants.HostTenant.Id));
        }

        var user = await usersQuery
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null || !user.Roles.Any())
        {
            return new HashSet<string>();
        }

        // Flatten into a distinct set of permissions from all roles
        var permissions = user.Roles
            .Where(r => r.IsActive)
            .SelectMany(r => r.Permissions)
            .ToHashSet();

        return permissions;
    }
}
