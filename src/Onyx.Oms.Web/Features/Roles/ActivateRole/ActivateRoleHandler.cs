using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Messaging;
using Onyx.Oms.Infrastructure.Identity.IdP;
using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Domain.Models;

namespace Onyx.Oms.Web.Features.Roles.ActivateRole;

public class ActivateRoleHandler : ICommandHandler<ActivateRoleCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityProviderApi _idpApi;
    private readonly ICurrentUserService _currentUserService;

    public ActivateRoleHandler(IApplicationDbContext context, IIdentityProviderApi idpApi, ICurrentUserService currentUserService)
    {
        _context = context;
        _idpApi = idpApi;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(ActivateRoleCommand request, CancellationToken cancellationToken)
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
            var response = await _idpApi.ActivateRoleAsync(role.Name, new RoleStatusRequest(targetClientId));
            if (!response.IsSuccessStatusCode)
            {
                 return Result.Failure(Error.Failure("Identity.ActivationFailed", $"IdP failed to activate role. Status: {response.StatusCode}"));
            }
        }
        catch (Exception ex)
        {
            return Result.Failure(Error.Failure("Identity.Connection", $"Failed to connect to IdP: {ex.Message}"));
        }

        role.Activate();
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
