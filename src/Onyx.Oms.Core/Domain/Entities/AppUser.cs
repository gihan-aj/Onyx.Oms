using Onyx.Oms.Core.Common.Models;

namespace Onyx.Oms.Core.Domain.Entities;

public class AppUser : AuditableEntity<int>
{
    public string IdentityUserId { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string DisplayName { get; private set; } = string.Empty;
    public DateTimeOffset? LastLoginUtc { get; private set; }
    
    // Stored as JSON via PrimitiveCollection
    private readonly List<string> _roles = new();
    public IReadOnlyCollection<string> Roles => _roles.AsReadOnly();

    // EF Core constructor
    private AppUser() : base(0) { }

    private AppUser(string identityUserId, string email, string displayName) : base(0)
    {
        IdentityUserId = identityUserId;
        Email = email;
        DisplayName = displayName;
        LastLoginUtc = DateTimeOffset.UtcNow;
    }

    public static AppUser Create(string identityUserId, string email, string displayName)
    {
        // Simple create, validation can be added if needed
        return new AppUser(identityUserId, email, displayName);
    }

    public void UpdateDetails(string email, string displayName)
    {
        Email = email;
        DisplayName = displayName;
    }

    public void RecordLogin()
    {
        LastLoginUtc = DateTimeOffset.UtcNow;
    }

    public void AssignRole(string roleName)
    {
        if (!_roles.Contains(roleName))
        {
            _roles.Add(roleName);
        }
    }

    public void RemoveRole(string roleName)
    {
        if (_roles.Contains(roleName))
        {
            _roles.Remove(roleName);
        }
    }
}
