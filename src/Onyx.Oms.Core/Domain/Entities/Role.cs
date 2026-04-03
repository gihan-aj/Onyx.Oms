using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Models;

namespace Onyx.Oms.Core.Domain.Entities;

public class Role : AuditableEntity<Guid>, IMustHaveTenant
{
    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; } = true;

    // Permissions stored as JSON
    private readonly List<string> _permissions = new();
    public IReadOnlyCollection<string> Permissions => _permissions.AsReadOnly();
    
    // Navigation property for Many-to-Many
    private readonly List<AppUser> _users = new();
    public IReadOnlyCollection<AppUser> Users => _users.AsReadOnly();

    private Role() : base(Guid.NewGuid()) { } // EF Core

    private Role(Guid tenantId, string name, string? description) : base(Guid.NewGuid())
    {
        TenantId = tenantId;
        Name = name;
        Description = description;
        IsActive = true;
    }

    public static Result<Role> Create(Guid tenantId, string name, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<Role>(Error.Validation("Role.NameRequired", "Role name is required."));

        return new Role(tenantId, name, description);
    }

    public void Update(string name, string? description)
    {
        Name = name;
        Description = description;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;

    public void AddPermission(string permission)
    {
        if (!_permissions.Contains(permission)) _permissions.Add(permission);
    }

    public void RemovePermission(string permission)
    {
        if (_permissions.Contains(permission)) _permissions.Remove(permission);
    }
}
