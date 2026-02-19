using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Entities;
using Onyx.Oms.Core.Messaging;
using Onyx.Oms.Infrastructure.Identity.IdP;
using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Domain.Models;

namespace Onyx.Oms.Web.Features.Roles.CreateRole;

public class CreateRoleHandler : ICommandHandler<CreateRoleCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityProviderApi _idpApi;
    private readonly ICurrentUserService _currentUserService;

    public CreateRoleHandler(IApplicationDbContext context, IIdentityProviderApi idpApi, ICurrentUserService currentUserService)
    {
        _context = context;
        _idpApi = idpApi;
        _currentUserService = currentUserService;
    }

    public async Task<Result<Guid>> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        // 1. Check if Role exists locally
        if (await _context.Roles.AnyAsync(r => r.Name == request.Name, cancellationToken))
        {
            return Result.Failure<Guid>(Error.Conflict("Role.Exists", $"Role '{request.Name}' already exists."));
        }

        // 2. Determine Target Client ID (The User's Client)
        var targetClientId = _currentUserService.ClientId;
        
        if (string.IsNullOrEmpty(targetClientId))
        {
             return Result.Failure<Guid>(Error.Failure("Identity.ClientIdMissing", 
                 "Could not determine Client ID from current user context."));
        }

        // 3. Call IdP to create role
        try
        {
            var idpResponse = await _idpApi.CreateRoleAsync(new CreateRoleRequest(request.Name, targetClientId));
            
            if (!idpResponse.IsSuccessStatusCode)
            {
                return Result.Failure<Guid>(Error.Failure("Identity.RoleCreation", 
                    $"Failed to create role in IdP. Status: {idpResponse.StatusCode}"));
            }
        }
        catch (Exception ex)
        {
             return Result.Failure<Guid>(Error.Failure("Identity.Connection", 
                 $"Failed to connect to Identity Provider: {ex.Message}"));
        }

        // 4. Create Local Role
        var role = Role.Create(request.Name, request.Description);

        // 5. Add Permissions
        foreach (var perm in request.Permissions)
        {
             role.AddPermission(perm);
        }

        _context.Roles.Add(role);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(role.Id);
    }
}
