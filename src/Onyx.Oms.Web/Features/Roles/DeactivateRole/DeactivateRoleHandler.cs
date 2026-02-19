using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Messaging;
using Onyx.Oms.Infrastructure.Identity.IdP;
using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Domain.Models;

namespace Onyx.Oms.Web.Features.Roles.DeactivateRole;

public class DeactivateRoleHandler : ICommandHandler<DeactivateRoleCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityProviderApi _idpApi;
    private readonly ICurrentUserService _currentUserService;

    public DeactivateRoleHandler(IApplicationDbContext context, IIdentityProviderApi idpApi, ICurrentUserService currentUserService)
    {
        _context = context;
        _idpApi = idpApi;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(DeactivateRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);
        if (role == null)
        {
            return Result.Failure(Error.NotFound("Role.NotFound", $"Role with Id {request.Id} was not found."));
        }

        var targetClientId = _currentUserService.ClientId;
        if (string.IsNullOrEmpty(targetClientId))
        {
            return Result.Failure(Error.Failure("Identity.ClientIdMissing", "Could not determine Client ID."));
        }

        try
        {
            var response = await _idpApi.DeactivateRoleAsync(role.Name, new RoleStatusRequest(targetClientId));
            if (!response.IsSuccessStatusCode)
            {
                 return Result.Failure(Error.Failure("Identity.DeactivationFailed", $"IdP failed to deactivate role. Status: {response.StatusCode}"));
            }
        }
        catch (Exception ex)
        {
            return Result.Failure(Error.Failure("Identity.Connection", $"Failed to connect to IdP: {ex.Message}"));
        }

        role.Deactivate();
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
