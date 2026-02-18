using Onyx.Oms.Core.Common.Models;

namespace Onyx.Oms.Core.Domain.Entities;

public class Role : AuditableEntity<Guid>
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    // Stored as JSON via PrimitiveCollection
    private readonly List<string> _permissions = new();
    public IReadOnlyCollection<string> Permissions => _permissions.AsReadOnly();

    // EF Core constructor
    private Role() : base(Guid.Empty) { }

    private Role(Guid id, string name, string? description) : base(id)
    {
        Name = name;
        Description = description;
    }

    public static Role Create(string name, string? description = null)
    {
        return new Role(Guid.NewGuid(), name, description);
    }

    public void Update(string name, string? description)
    {
        Name = name;
        Description = description;
    }

    public void AddPermission(string permission)
    {
        if (!_permissions.Contains(permission))
        {
            _permissions.Add(permission);
        }
    }

    public void RemovePermission(string permission)
    {
        if (_permissions.Contains(permission))
        {
            _permissions.Remove(permission);
        }
    }
}
