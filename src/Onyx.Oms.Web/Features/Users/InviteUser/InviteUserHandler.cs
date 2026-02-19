using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Entities;
using Onyx.Oms.Core.Messaging;
using Onyx.Oms.Infrastructure.Identity.IdP;
using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Domain.Models;

namespace Onyx.Oms.Web.Features.Users.InviteUser;

public class InviteUserHandler : ICommandHandler<InviteUserCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityProviderApi _idpApi;
    private readonly ICurrentUserService _currentUserService;

    public InviteUserHandler(IApplicationDbContext context, IIdentityProviderApi idpApi, ICurrentUserService currentUserService)
    {
        _context = context;
        _idpApi = idpApi;
        _currentUserService = currentUserService;
    }

    public async Task<Result<Guid>> Handle(InviteUserCommand request, CancellationToken cancellationToken)
    {
        // 1. Validate Role exists locally using Guid ID
        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == request.RoleId, cancellationToken);
        if (role == null)
        {
            return Result.Failure<Guid>(Error.NotFound("Role.NotFound", $"Role with Id {request.RoleId} was not found."));
        }

        // 2. Get Target Client ID
        var targetClientId = _currentUserService.ClientId;
        if (string.IsNullOrEmpty(targetClientId))
        {
            return Result.Failure<Guid>(Error.Failure("Identity.ClientIdMissing", "Could not determine Client ID."));
        }

        // 3. Call IdP to Invite User and Assign Role
        // The IdP's InviteUser endpoint handles creating the user (if new) and assigning the role.
        Guid userId = Guid.Empty;

        try
        {
            var idpRequest = new InviteUserRequest(
                request.Email,
                role.Name, // Pass the role name (e.g., "OrderSystem_Admin")
                request.FirstName,
                request.LastName,
                targetClientId
            );

            var response = await _idpApi.InviteUserAsync(idpRequest);

            if (!response.IsSuccessStatusCode)
            {
                return Result.Failure<Guid>(Error.Failure("Identity.InviteFailed", $"IdP failed to invite user. Status: {response.StatusCode}"));
            }

            if (response.Content == null)
            {
                 return Result.Failure<Guid>(Error.Failure("Identity.InvalidResponse", "IdP returned null content."));
            }

            userId = response.Content.Id;
        }
        catch (Exception ex)
        {
            return Result.Failure<Guid>(Error.Failure("Identity.Connection", $"Failed to connect to IdP: {ex.Message}"));
        }

        // 4. Create or Update Local User Mirror
        // We sync the user locally so they appear in lists immediately
        var localUser = await _context.AppUsers
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken); // Match by Email or IdentityUserId if we had it initially

        if (localUser == null)
        {
            // Create new local user
            // Note: AppUser.Id should probably match IdP User Id if possible, or be a new local Guid?
            // Domain Entity AppUser is `AuditableEntity<Guid>`.
            // The `InviteUserAsync` returns `UserDto` with `Id`. We should use that `Id`.
            // But `AppUser` constructor or factory might need adjustment if it generates a new Guid by default.
            // Let's check AppUser.Create.
            
            // AppUser.Create(string identityUserId, string email, string firstName, string? lastName) -> new AppUser(Guid.NewGuid()...)
            // We want to use the GUID from the IdP if our system shares the same GUIDs or if IdentityUserId is the link.
            // In `AppUser.cs`:
            // `public string IdentityUserId { get; private set; }`
            // `private AppUser(Guid id, ...)` called by Create.
            // We should use the IdP's ID as `IdentityUserId`. The local `Id` (PK) can be generated or same.
            // Ideally, to avoid confusion, let's keep local ID auto-generated or separate, and link via IdentityUserId.
            
            localUser = AppUser.Create(userId.ToString(), request.Email, request.FirstName, request.LastName);
            _context.AppUsers.Add(localUser);
        }
        else 
        {
            // Update existing
            localUser.Update(request.FirstName, request.LastName);
        }

        // 5. Assign Role Locally
        // Verify we aren't duplicating
        if (!localUser.Roles.Any(r => r.Id == role.Id))
        {
            localUser.AssignRole(role);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(localUser.Id);
    }
}
