using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Domain.Constants;
using Onyx.Oms.Core.Domain.Entities;
using Onyx.Oms.Core.Domain.ValueObjects;

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

                var allTenants = await _context.Tenants.ToListAsync(cancellationToken);
                foreach (var tenant in allTenants)
                {
                    // Add payment configs for older tenants
                    var hasPaymentConfigs = await _context.PaymentMethodConfigs
                        .AnyAsync(p => p.TenantId == tenant.Id, cancellationToken);

                    if (!hasPaymentConfigs)
                    {
                        var defaultConfigs = DefaultPaymentMethods.GetConfigs(tenant.Id);
                        _context.PaymentMethodConfigs.AddRange(defaultConfigs);
                    }

                    // Add sl post courier for older tenants
                    // Add SLPost as a courier
                    if (!await _context.Couriers.AnyAsync(c => c.TenantId == tenant.Id && c.ProviderType == Core.Domain.Enums.CourierProviderType.SLPost, cancellationToken))
                    {
                        var name = "SL Post";

                        var existing = await _context.Couriers.FirstOrDefaultAsync(c => c.TenantId == tenant.Id && c.Name == name, cancellationToken);
                        if(existing == null)
                        {
                            var slPostResult = Courier.Create(
                                tenant.Id,
                                "SL Post",
                                null, null, null,
                                "https://slpost.gov.lk/cash-on-delivery-service/",
                                null,
                                Core.Domain.Enums.CourierProviderType.SLPost,
                                true);

                            if (slPostResult.IsSuccess)
                                _context.Couriers.Add(slPostResult.Value);
                        }
                        else
                        {
                            existing.UpdateDetails(
                                existing.Name,
                                existing.ContactPerson,
                                existing.PrimaryPhone,
                                existing.SecondaryPhone,
                                existing.WebsiteUrl,
                                existing.TrackingUrlTemplate,
                                Core.Domain.Enums.CourierProviderType.SLPost,
                                true);
                        }
                        
                    }
                }

                await _context.SaveChangesAsync(cancellationToken);

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

                // Populating empty money fields in OrderPayment
                var payments = await _context.OrderPayments
                    .Where(p => p.Received.Amount == 0)
                    .ToListAsync(cancellationToken);
                foreach (var payment in payments)
                {
                    var config = await _context.PaymentMethodConfigs
                        .FirstOrDefaultAsync(pc => pc.Type == payment.Method, cancellationToken);
                    payment.TempUpdateReceived(config?.FeeRate ?? 0m);
                }

                var orderItemsWithoutWeight = await _context.OrderItems
                    .Where(oi => oi.UnitWeight == null)
                    .ToListAsync(cancellationToken);

                foreach (var orderItem in orderItemsWithoutWeight)
                {
                    // Get the product variant to fetch its weight
                    var variant = await _context.ProductVariants
                        .FirstOrDefaultAsync(pv => pv.Id == orderItem.ProductVariantId, cancellationToken);

                    if (variant?.Weight != null)
                    {
                        orderItem.UpdateWeight(new Weight(variant.Weight.Value, variant.Weight.Unit));
                    }
                    else
                    {
                        // Default to 0 kg if no weight available
                        orderItem.UpdateWeight(Weight.Zero());
                    }
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
