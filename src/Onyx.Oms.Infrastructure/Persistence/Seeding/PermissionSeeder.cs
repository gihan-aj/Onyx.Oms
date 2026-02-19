using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Onyx.Oms.Core.Domain.Constants;
using Onyx.Oms.Core.Domain.Entities;
using Onyx.Oms.Infrastructure.Identity;
using Onyx.Oms.Infrastructure.Identity.IdP;

namespace Onyx.Oms.Infrastructure.Persistence.Seeding;

public class PermissionSeeder
{
    private readonly AppDbContext _context;
    private readonly IIdentityProviderApi _idpApi;
    private readonly AuthenticationOptions _authOptions;

    public PermissionSeeder(
        AppDbContext context, 
        IIdentityProviderApi idpApi,
        IOptions<AuthenticationOptions> authOptions)
    {
        _context = context;
        _idpApi = idpApi;
        _authOptions = authOptions.Value;
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
                await _idpApi.CreateRoleAsync(new CreateRoleRequest(superAdminRoleName, _authOptions.ClientId));
            }
            catch 
            {
                // Likely already exists in IdP, ignore
            }
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
