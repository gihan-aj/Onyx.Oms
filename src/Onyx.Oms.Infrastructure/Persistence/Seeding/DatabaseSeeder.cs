using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Domain.Constants;
using Onyx.Oms.Core.Domain.Entities;

namespace Onyx.Oms.Infrastructure.Persistence.Seeding
{
    public class DatabaseSeeder
    {
        private readonly AppDbContext _context;

        public DatabaseSeeder(AppDbContext context)
        {
            _context = context;
        }

        public async Task SeedAsync(CancellationToken cancellationToken = default)
        {
            await _context.Database.MigrateAsync();

            var hostTenant = await _context.Tenants
                .FirstOrDefaultAsync(t => t.Id == Tenants.HostTenant.Id, cancellationToken);
            if (hostTenant == null)
            {
                var tenantResult = Tenant.Create(
                    companyName: Tenants.HostTenant.Name,
                    contactEmail: Users.SystemAdmin.Email,
                    contactPhone: null,
                    explicitId: Tenants.HostTenant.Id);

                hostTenant = tenantResult.Value;
                _context.Tenants.Add(hostTenant);
                await _context.SaveChangesAsync(cancellationToken);
            }

            await SeedRolesAndPermissionsAsync(cancellationToken);

            var adminUser = await _context.AppUsers
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.Id == Users.SystemAdmin.Id, cancellationToken);

            if(adminUser == null)
            {
                var userResult = AppUser.Create(
                    identityUserId: Users.SystemAdmin.Id,
                    tenantId: Tenants.HostTenant.Id,
                    email: Users.SystemAdmin.Email,
                    firstName: "System",
                    lastName: "Admin");

                adminUser = userResult.Value;
                _context.AppUsers.Add(adminUser);
            }

            var systemAdminRole = await _context.Set<Role>().FirstAsync(r => r.Name == Roles.Oms.SystemAdmin, cancellationToken);

            if (!adminUser.Roles.Any(r => r.Name == Roles.Oms.SystemAdmin))
            {
                adminUser.AssignRole(systemAdminRole);
            }

            await _context.SaveChangesAsync(cancellationToken);
        }

        private async Task SeedRolesAndPermissionsAsync(CancellationToken cancellationToken)
        {
            var allPermssions = Permissions.GetAllPermissions();

            var systemPermissions = allPermssions.Where(p => p.StartsWith("system:")).ToList();
            var tenantPermissions = allPermssions.Where(p => p.StartsWith("tenant:")).ToList();

            var systemAdminRole = await _context.Roles
                .Include(p => p.Permissions)
                .FirstOrDefaultAsync(r => r.Name == Roles.Oms.SystemAdmin, cancellationToken);

            if(systemAdminRole == null)
            {
                systemAdminRole = Role.Create(Roles.Oms.SystemAdmin, "God Mode Role - Has all the permissions");
                _context.Roles.Add(systemAdminRole);
            }

            SyncPermissions(systemAdminRole, allPermssions);

            var tenantOwnerRole = await _context.Roles
                .Include(p => p.Permissions)
                .FirstOrDefaultAsync(r => r.Name == Roles.Oms.TenantOwner, cancellationToken);

            if(tenantOwnerRole == null)
            {
                tenantOwnerRole = Role.Create(Roles.Oms.TenantOwner, "Full tenant access");
                _context.Roles.Add(tenantOwnerRole);
            }

            SyncPermissions(tenantOwnerRole, tenantPermissions);
        }

        private void SyncPermissions(Role role, List<string> intendedPermissions)
        {
            var currentPermissions = role.Permissions.ToList();

            var missingPermissions = intendedPermissions.Except(currentPermissions).ToList();
            var obsoletePermissios = currentPermissions.Except(intendedPermissions).ToList();

            foreach(var perm in missingPermissions)
            {
                role.AddPermission(perm);
            }

            foreach(var perm in obsoletePermissios)
            {
                role.RemovePermission(perm);
            }
        }
    }
}
