using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Messaging;
using Onyx.Oms.Infrastructure.Identity.IdP;
using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Domain.Models;

namespace Onyx.Oms.Web.Features.Roles.UpdateRole;

public class UpdateRoleHandler : ICommandHandler<UpdateRoleCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityProviderApi _idpApi;
    private readonly ICurrentUserService _currentUserService;

    public UpdateRoleHandler(IApplicationDbContext context, IIdentityProviderApi idpApi, ICurrentUserService currentUserService)
    {
        _context = context;
        _idpApi = idpApi;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (role == null)
        {
            return Result.Failure(Error.NotFound("Role.NotFound", $"Role with Id {request.Id} was not found."));
        }

        // Do not let edit SuperAdmin role
        if (role.Name == "SuperAdmin")
        {
            return Result.Failure(Error.Unauthorized("Role.EditForbidden", "Editing the SuperAdmin role is not allowed."));
        }

        // Handle Renaming
        if (role.Name != request.Name)
        {
            var targetClientId = _currentUserService.ClientId;
            if (string.IsNullOrEmpty(targetClientId))
            {
                return Result.Failure(Error.Failure("Identity.ClientIdMissing", "Could not determine Client ID for renaming."));
            }

            try
            {
                // Call IdP to rename
                var response = await _idpApi.UpdateRoleNameAsync(role.Name, new UpdateRoleNameRequest(request.Name, targetClientId));
                
                if (!response.IsSuccessStatusCode)
                {
                     return Result.Failure(Error.Failure("Identity.RenameFailed", $"IdP failed to rename role. Status: {response.StatusCode}"));
                }
            }
            catch (Exception ex)
            {
                return Result.Failure(Error.Failure("Identity.Connection", $"Failed to connect to IdP: {ex.Message}"));
            }
        }

        // Update Local Details
        role.Update(request.Name, request.Description);

        // Update Permissions
        // 1. Add new permissions
        foreach (var perm in request.Permissions)
        {
            role.AddPermission(perm);
        }

        // 2. Remove permissions not in the list
        var currentPermissions = role.Permissions.ToList();
        foreach (var perm in currentPermissions)
        {
            if (!request.Permissions.Contains(perm))
            {
                role.RemovePermission(perm);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
