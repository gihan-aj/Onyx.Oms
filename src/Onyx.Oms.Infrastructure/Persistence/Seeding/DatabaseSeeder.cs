using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Domain.Constants;
using Onyx.Oms.Core.Domain.Entities;

namespace Onyx.Oms.Infrastructure.Persistence.Seeding
{
    public class DatabaseSeeder
    {
        private readonly AppDbContext _context;
        private readonly ITenantSecurityBypass _bypass;

        public DatabaseSeeder(AppDbContext context, ITenantSecurityBypass bypass)
        {
            _context = context;
            _bypass = bypass;
        }

        public async Task SeedAsync(CancellationToken cancellationToken = default)
        {
            using (_bypass.EnableBypass())
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

                await SeedRolesAndPermissionsAsync(hostTenant.Id, cancellationToken);

                var adminUser = await _context.AppUsers
                    .Include(u => u.Roles)
                    .FirstOrDefaultAsync(u => u.Id == Users.SystemAdmin.Id, cancellationToken);

                if (adminUser == null)
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

                var systemAdminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == Roles.Oms.SystemAdmin, cancellationToken);

                if (!adminUser.Roles.Any(r => r.Name == Roles.Oms.SystemAdmin))
                {
                    adminUser.AssignRole(systemAdminRole!);
                }

                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        private async Task SeedRolesAndPermissionsAsync(Guid hostTenantId, CancellationToken cancellationToken)
        {
            var allPermssions = Permissions.GetAllPermissions();

            var systemPermissions = allPermssions.Where(p => p.StartsWith("system:")).ToList();
            var tenantPermissions = allPermssions.Where(p => p.StartsWith("tenant:")).ToList();

            var systemAdminRole = await _context.Roles
                .FirstOrDefaultAsync(r => r.Name == Roles.Oms.SystemAdmin, cancellationToken);

            if(systemAdminRole == null)
            {
                var systemAdminRoleResult = Role.Create(hostTenantId, Roles.Oms.SystemAdmin, "God Mode Role - Has all the permissions");
                systemAdminRole = systemAdminRoleResult.Value;
                _context.Roles.Add(systemAdminRole);
            }

            SyncPermissions(systemAdminRole, allPermssions);

            var tenantOwnerRole = await _context.Roles
                .FirstOrDefaultAsync(r => r.Name == Roles.Oms.TenantOwner, cancellationToken);

            if(tenantOwnerRole == null)
            {
                var tenantOwnerRoleResult = Role.Create(hostTenantId, Roles.Oms.TenantOwner, "Full tenant access");
                tenantOwnerRole = tenantOwnerRoleResult.Value;
                _context.Roles.Add(tenantOwnerRole);
            }

            SyncPermissions(tenantOwnerRole, tenantPermissions);

            await _context.SaveChangesAsync(cancellationToken);
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
