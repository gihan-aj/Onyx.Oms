using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Domain.Constants;
using Onyx.Oms.Core.Domain.Entities;
using Onyx.Oms.Infrastructure.Identity.IdP;

namespace Onyx.Oms.Infrastructure.Persistence.Seeding;

public class PermissionSeeder
{
    private readonly AppDbContext _context;
    private readonly IIdentityProviderApi _idpApi;

    public PermissionSeeder(AppDbContext context, IIdentityProviderApi idpApi)
    {
        _context = context;
        _idpApi = idpApi;
    }

    public async Task SeedAsync()
    {
        // 1. Get all defined permissions via reflection
        var allPermissions = GetAllDefinedPermissions();

        // 2. Ensure SuperAdmin role exists locally
        var superAdminRoleName = "SuperAdmin";
        var superAdminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == superAdminRoleName);

        if (superAdminRole == null)
        {
            // Create locally if not exists
            superAdminRole = Role.Create(superAdminRoleName, "God Mode Role - Has all permissions");
            _context.Roles.Add(superAdminRole);
            
            // Try to create in IdP (fire and forget or log error if exists)
            try 
            {
                var response = await _idpApi.CreateRoleAsync(new CreateRoleRequest(superAdminRoleName));
            }
            catch 
            {
                // Likely already exists in IdP, ignore
            }
        }

        // 3. Sync Permissions: Ensure SuperAdmin has ALL permissions
        // We do this every startup to ensure new permissions are added automatically
        var currentPermissions = new HashSet<string>(superAdminRole.Permissions);
        var missingPermissions = allPermissions.Except(currentPermissions).ToList();
        var obsoletePermissions = currentPermissions.Except(allPermissions).ToList();

        if (missingPermissions.Any() || obsoletePermissions.Any())
        {
            foreach (var perm in missingPermissions)
            {
                superAdminRole.AddPermission(perm);
            }

            // Optional: Remove obsolete permissions? 
            // Better to keep them in case code is rolled back, but for "SuperAdmin" standardizing is fine.
            foreach (var perm in obsoletePermissions)
            {
                superAdminRole.RemovePermission(perm);
            }
            
            await _context.SaveChangesAsync();
        }
    }

    private static HashSet<string> GetAllDefinedPermissions()
    {
        var permissions = new HashSet<string>();
        var rootType = typeof(Permissions);

        // Iterate through nested classes (Users, Roles, Couriers, etc.)
        foreach (var nestedType in rootType.GetNestedTypes(BindingFlags.Public | BindingFlags.Static))
        {
            foreach (var field in nestedType.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy))
            {
                if (field.IsLiteral && !field.IsInitOnly && field.FieldType == typeof(string))
                {
                    if (field.GetValue(null) is string value)
                    {
                        permissions.Add(value);
                    }
                }
            }
        }

        return permissions;
    }
}
