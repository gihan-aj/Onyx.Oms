using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Messaging;
using Onyx.Oms.Web.Features.Settings.TenantProfile.GetProfile;

namespace Onyx.Oms.Web.Features.Settings.TenantProfile.UpdateRegionalSettings;

public class UpdateRegionalSettingsHandler : ICommandHandler<UpdateRegionalSettingsCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateRegionalSettingsHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(UpdateRegionalSettingsCommand request, CancellationToken cancellationToken)
    {
        var profile = await _context.Tenants.FirstOrDefaultAsync(cancellationToken);

        if (profile == null)
        {
            return Result.Failure<TenantProfileDto>(Error.NotFound("Tenant.NotFound", "Tenant details not found."));
        }

        profile.UpdateRegionalSettings(request.DefaultCurrency, request.TimeZone, request.WeightUnit);

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
