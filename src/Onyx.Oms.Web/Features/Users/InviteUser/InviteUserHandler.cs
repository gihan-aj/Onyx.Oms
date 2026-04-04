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
        Guid? tenantId = _currentUserService.ActiveTenantId;
        if (tenantId == null)
            return Result.Failure<Guid>(Error.Unauthorized("Product.TenantIdMissing", "Tenant Id not found."));

        // 1. Validate Roles exist locally using Guid IDs
        var requestRoleIds = request.RoleIds.Distinct().ToList();
        var roles = await _context.Roles.Where(r => requestRoleIds.Contains(r.Id)).ToListAsync(cancellationToken);
        if (roles.Count != requestRoleIds.Count)
        {
            return Result.Failure<Guid>(Error.NotFound("Role.NotFound", "One or more specified roles were not found."));
        }

        // 3. Call IdP to Invite User and Assign Roles
        // The IdP's InviteUser endpoint handles creating the user (if new) and assigning the roles.
        Guid userId = Guid.Empty;
        IEnumerable<string> assignedRoleNames = Enumerable.Empty<string>();

        try
        {
            var idpRequest = new InviteUserRequest(
                request.Email,
                request.FirstName,
                request.LastName,
                tenantId.Value
            );

            var response = await _idpApi.InviteUserAsync(idpRequest);

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                {
                    //// User already exists in IdP, gracefully handle by fetching the user and assigning roles
                    //var userResponse = await _idpApi.GetUserByEmailAsync(request.Email);
                    //if (!userResponse.IsSuccessStatusCode || userResponse.Content == null)
                    //{
                    //    return Result.Failure<Guid>(Error.Failure("Identity.UserFetchFailed", $"Failed to fetch existing user. Status: {userResponse.StatusCode}"));
                    //}
                    
                    //userId = userResponse.Content.Id;

                    //// Assign roles to existing user
                    //var assignRolesRequest = new AssignRolesRequest(roles.Select(r => r.Name), targetClientId);
                    //var assignResponse = await _idpApi.AssignRolesAsync(userId, assignRolesRequest);

                    //if (!assignResponse.IsSuccessStatusCode || assignResponse.Content == null)
                    //{
                    //     return Result.Failure<Guid>(Error.Failure("Identity.RoleAssignmentFailed", $"IdP failed to assign roles to existing user. Status: {assignResponse.StatusCode}"));
                    //}

                    //assignedRoleNames = assignResponse.Content.AssignedRoles ?? Enumerable.Empty<string>();
                }
                else
                {
                    return Result.Failure<Guid>(Error.Failure("Identity.InviteFailed", $"IdP failed to invite user. Status: {response.StatusCode}"));
                }
            }
            else
            {
                if (response.Content == null)
                {
                    return Result.Failure<Guid>(Error.Failure("Identity.InvalidResponse", "IdP returned null content."));
                }

                userId = response.Content.Id;
                assignedRoleNames = response.Content.AssignedRoles ?? Enumerable.Empty<string>();
            }
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
            var localUserResult = AppUser.Create(userId, tenantId.Value, request.FirstName, request.LastName);
            if(localUserResult.IsFailure)
                return Result.Failure<Guid>(localUserResult.Error);

            localUser = localUserResult.Value;

            _context.AppUsers.Add(localUser);
        }
        else 
        {
            // Update existing
            localUser.Update(request.FirstName, request.LastName);
        }

        // 5. Assign Roles Locally
        // Verify we aren't duplicating and only assign roles the IDP successfully assigned
        var rolesToAssignLocally = roles.Where(r => assignedRoleNames.Contains(r.Name)).ToList();

        foreach (var role in rolesToAssignLocally)
        {
            if (!localUser.Roles.Any(r => r.Id == role.Id))
            {
                localUser.AssignRole(role);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(localUser.Id);
    }
}
