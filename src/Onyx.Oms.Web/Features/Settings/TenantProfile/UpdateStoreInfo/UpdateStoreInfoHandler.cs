using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Messaging;
using Onyx.Oms.Web.Features.Settings.TenantProfile.GetProfile;

namespace Onyx.Oms.Web.Features.Settings.TenantProfile.UpdateStoreInfo;

public class UpdateStoreInfoHandler : ICommandHandler<UpdateStoreInfoCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public UpdateStoreInfoHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(UpdateStoreInfoCommand request, CancellationToken cancellationToken)
    {
        var profile = await _context.Tenants
            .Where(t => t.Id == _currentUserService.ActiveTenantId)
            .FirstOrDefaultAsync(cancellationToken);

        if (profile == null)
        {
            return Result.Failure<TenantProfileDto>(Error.NotFound("Tenant.NotFound", "Tenant details not found."));
        }

        profile.UpdateStoreInfo(
            request.StoreName, 
            request.LegalName, 
            request.TaxRegistrationNumber, 
            request.ContactEmail, 
            request.ContactPhone,
            request.InvoiceFooterText);

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
