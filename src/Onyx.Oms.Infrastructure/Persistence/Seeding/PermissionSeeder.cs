using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Domain.Constants;
using Onyx.Oms.Core.Domain.Entities;

namespace Onyx.Oms.Infrastructure.Persistence.Seeding;

public class PermissionSeeder
{
    private readonly IApplicationDbContext _context;

    public PermissionSeeder(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        // 1. Get all defined permissions from Constants
        var allPermissions = Permissions.GetAllPermissions();

        // 2. Ensure SuperAdmin role exists locally
        var superAdminRoleName = "SuperAdmin";
        var superAdminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == superAdminRoleName);

        if (superAdminRole == null)
        {
            // Create locally if not exists
            superAdminRole = Role.Create(superAdminRoleName, "God Mode Role - Has all permissions");
            _context.Roles.Add(superAdminRole);
            
            // NOTE: We do NOT sync this role to IdP automatically.
            // Why? Because we don't know the Front-end Client ID here (this runs in backend context),
            // so we can't create the role with the correct prefix (e.g. "OrderSystem_SuperAdmin").
            // The IdP Admin should manually create this role for the Frontend Client if needed.
        }

        // 3. Sync Permissions: Ensure SuperAdmin has ALL permissions
        var currentPermissions = new HashSet<string>(superAdminRole.Permissions);
        var missingPermissions = allPermissions.Except(currentPermissions).ToList();
        var obsoletePermissions = currentPermissions.Except(allPermissions).ToList();

        if (missingPermissions.Any() || obsoletePermissions.Any())
        {
            foreach (var perm in missingPermissions)
            {
                superAdminRole.AddPermission(perm);
            }

            // Optional: Remove obsolete permissions if they were removed from code constants
            foreach (var perm in obsoletePermissions)
            {
                superAdminRole.RemovePermission(perm);
            }
            
            // If the role was just added, it might not track changes correctly without this?
            // Actually EF Core tracks the entity. 
        }
        
        // Ensure SuperAdmin is active
        if (!superAdminRole.IsActive)
        {
            superAdminRole.Activate();
        }

        await _context.SaveChangesAsync(CancellationToken.None);
    }
}
