using MediatR;
using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Entities;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Infrastructure.Identity.IdP;

namespace Onyx.Oms.Web.Features.Roles.CreateRole;

public class CreateRoleHandler : IRequestHandler<CreateRoleCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityProviderApi _idpApi;

    public CreateRoleHandler(IApplicationDbContext context, IIdentityProviderApi idpApi)
    {
        _context = context;
        _idpApi = idpApi;
    }

    public async Task<Result<Guid>> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        // 1. Check if Role exists locally
        if (await _context.Roles.AnyAsync(r => r.Name == request.Name, cancellationToken))
        {
            return Result.Failure<Guid>(Error.Conflict("Role.Exists", $"Role '{request.Name}' already exists."));
        }

        // 2. Call IdP to create role
        // We do this BEFORE saving locally. If IdP fails, we abort.
        // IdP handles idempotency (if exists, it might return success or we handle error). 
        // Our IdP API wrapper returns IApiResponse.
        try
        {
            var idpResponse = await _idpApi.CreateRoleAsync(new CreateRoleRequest(request.Name));
            
            if (!idpResponse.IsSuccessStatusCode)
            {
                // If 409 Conflict, it might be okay (already exists in IdP but not locally?) 
                // But for now, let's treat it as an error to be safe or sync issue.
                // Or maybe strictly fail.
                return Result.Failure<Guid>(Error.Failure("Identity.RoleCreation", 
                    $"Failed to create role in IdP. Status: {idpResponse.StatusCode}"));
            }
        }
        catch (Exception ex)
        {
             return Result.Failure<Guid>(Error.Failure("Identity.Connection", 
                 $"Failed to connect to Identity Provider: {ex.Message}"));
        }

        // 3. Create Local Role
        var role = Role.Create(request.Name, request.Description);

        // 4. Add Permissions
        // Validate permissions exist in our constant list? For now assumie trusted input or just string storage.
        // Ideally we validate against defined permissions.
        foreach (var perm in request.Permissions)
        {
             role.AddPermission(perm);
        }

        _context.Roles.Add(role);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(role.Id);
    }
}
