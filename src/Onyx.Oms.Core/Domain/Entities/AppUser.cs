using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Models;

namespace Onyx.Oms.Core.Domain.Entities;

public class AppUser : AuditableEntity<Guid>, IMustHaveTenant
{
    public Guid TenantId { get; private set; }
    public Tenant Tenant { get; private set; } = null!;

    public string Email { get; private set; } = string.Empty;
    public string FirstName { get; private set; } = string.Empty;
    public string? LastName { get; private set; } = string.Empty;
    public DateTime? LastLoginUtc { get; private set; }
    public bool IsActive { get; private set; }

    // Navigation property for Many-to-Many
    private readonly List<Role> _roles = new();
    public IReadOnlyCollection<Role> Roles => _roles.AsReadOnly();

    private AppUser() : base(Guid.Empty) { }

    private AppUser(Guid identityUserId, Guid tenantId, string email, string firstName, string? lastName)
    {
        Id = identityUserId;
        TenantId = tenantId;
        Email = email;
        FirstName = firstName;
        LastName = lastName;
        IsActive = true;
    }

    public static Result<AppUser> Create(Guid identityUserId, Guid tenantId, string email, string firstName, string? lastName = null)
    {
        if (identityUserId == Guid.Empty)
            return Result.Failure<AppUser>(Error.Validation("AppUser.IdentityUserIdRequired", "Identity User ID is required."));

        if (tenantId == Guid.Empty)
            return Result.Failure<AppUser>(Error.Validation("AppUser.TenantIdRequired", "Tenant ID is required."));

        if (string.IsNullOrWhiteSpace(email))
            return Result.Failure<AppUser>(Error.Validation("AppUser.EmailRequired", "Email is required."));

        if (string.IsNullOrWhiteSpace(firstName))
            return Result.Failure<AppUser>(Error.Validation("AppUser.FirstNameRequired", "First Name is required."));

        return Result.Success(new AppUser(identityUserId, tenantId, email, firstName, lastName));
    }

    public Result Update(string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            return Result.Failure(Error.Validation("AppUser.FirstNameRequired", "First Name is required."));

        FirstName = firstName;
        LastName = lastName;

        return Result.Success();
    }

    public void UpdateLastLoginTime()
    {
        LastLoginUtc = DateTime.UtcNow;
    }
    
    public Result AssignRole(Role role)
    {
        if (role == null)
            return Result.Failure(Error.Validation("AppUser.RoleRequired", "Role is required."));

        if (!_roles.Contains(role))
        {
            _roles.Add(role);
        }

        return Result.Success();
    }

    public Result RemoveRole(Role role)
    {
        if (role == null)
            return Result.Failure(Error.Validation("AppUser.RoleRequired", "Role is required."));

        if (_roles.Contains(role))
        {
            _roles.Remove(role);
        }

        return Result.Success();
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
