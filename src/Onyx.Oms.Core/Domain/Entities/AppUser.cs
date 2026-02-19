using Onyx.Oms.Core.Common.Models;

namespace Onyx.Oms.Core.Domain.Entities;

public class AppUser : AuditableEntity<Guid>
{
    public string IdentityUserId { get; private set; } = string.Empty; // Maps to Identity's User ID
    public string Email { get; private set; } = string.Empty;
    public string FirstName { get; private set; } = string.Empty;
    public string? LastName { get; private set; } = string.Empty;
    public DateTime? LastLoginUtc { get; private set; }
    public bool IsActive { get; private set; }

    // Navigation property for Many-to-Many
    private readonly List<Role> _roles = new();
    public IReadOnlyCollection<Role> Roles => _roles.AsReadOnly();

    private AppUser() : base(Guid.Empty) { }

    private AppUser(Guid id, string identityUserdId, string email, string firstName, string? lastName) : base(id)
    {
        IdentityUserId = identityUserdId;
        Email = email;
        FirstName = firstName;
        LastName = lastName;
        IsActive = true;
    }

    public static AppUser Create(string identityUserdId, string email, string firstName, string? lastName = null)
    {
        return new AppUser(Guid.NewGuid(), identityUserdId, email, firstName, lastName);
    }

    public void Update(string firstName, string lastName)
    {
        FirstName = firstName;
        LastName = lastName;
    }

    public void UpdateLastLoginTime()
    {
        LastLoginUtc = DateTime.UtcNow;
    }
    
    public void AssignRole(Role role)
    {
        if (!_roles.Contains(role))
        {
            _roles.Add(role);
        }
    }

    public void RemoveRole(Role role)
    {
        if (_roles.Contains(role))
        {
            _roles.Remove(role);
        }
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}
