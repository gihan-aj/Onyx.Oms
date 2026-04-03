using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Messaging;
using Onyx.Oms.Web.Common.Settings;
using Onyx.Oms.Web.Features.Settings.TenantProfile.GetProfile;

namespace Onyx.Oms.Web.Features.Settings.TenantProfile.UpdateStoreInfo;

public class UpdateStoreInfoHandler : ICommandHandler<UpdateStoreInfoCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly DefaultTenantProfileSettings _defaultSettings;

    public UpdateStoreInfoHandler(IApplicationDbContext context, IOptions<DefaultTenantProfileSettings> defaultSettings)
    {
        _context = context;
        _defaultSettings = defaultSettings.Value;
    }

    public async Task<Result> Handle(UpdateStoreInfoCommand request, CancellationToken cancellationToken)
    {
        var profile = await _context.Tenants.FirstOrDefaultAsync(cancellationToken);

        if (profile == null)
        {
            return Result.Failure<TenantProfileDto>(Error.NotFound("Tenant.NotFound", "Tenant details not found."));
        }

        profile.UpdateStoreInfo(
            request.StoreName, 
            request.LegalName, 
            request.TaxRegistrationNumber, 
            request.ContactEmail, 
            request.ContactPhone);

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
