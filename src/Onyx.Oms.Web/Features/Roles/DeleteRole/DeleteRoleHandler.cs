using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Messaging;
using Onyx.Oms.Infrastructure.Identity.IdP;
using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Domain.Models;

namespace Onyx.Oms.Web.Features.Roles.DeleteRole;

public class DeleteRoleHandler : ICommandHandler<DeleteRoleCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityProviderApi _idpApi;
    private readonly ICurrentUserService _currentUserService;

    public DeleteRoleHandler(IApplicationDbContext context, IIdentityProviderApi idpApi, ICurrentUserService currentUserService)
    {
        _context = context;
        _idpApi = idpApi;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);
        if (role == null)
        {
            return Result.Failure(Error.NotFound("Role.NotFound", $"Role with Id {request.Id} was not found."));
        }

        // Do not delete SuperAdmin role
        if (role.Name == "SuperAdmin")
        {
            return Result.Failure(Error.Unauthorized("Role.DeletionForbidden", "The SuperAdmin role cannot be deleted."));
        }

        var targetClientId = _currentUserService.ClientId;
        if (string.IsNullOrEmpty(targetClientId))
        {
            return Result.Failure(Error.Failure("Identity.ClientIdMissing", "Could not determine Client ID."));
        }

        try
        {
            var response = await _idpApi.DeleteRoleAsync(role.Name, targetClientId);
            
            if (!response.IsSuccessStatusCode && response.StatusCode != System.Net.HttpStatusCode.NotFound)
            {
                 return Result.Failure(Error.Failure("Identity.DeletionFailed", $"IdP failed to delete role. Status: {response.StatusCode}"));
            }
        }
        catch (Exception ex)
        {
            return Result.Failure(Error.Failure("Identity.Connection", $"Failed to connect to IdP: {ex.Message}"));
        }

        _context.Roles.Remove(role);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
