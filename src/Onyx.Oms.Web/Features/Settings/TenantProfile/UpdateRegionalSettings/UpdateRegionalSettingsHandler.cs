using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Messaging;
using Onyx.Oms.Web.Common.Settings;

namespace Onyx.Oms.Web.Features.Settings.TenantProfile.UpdateRegionalSettings;

public class UpdateRegionalSettingsHandler : ICommandHandler<UpdateRegionalSettingsCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly DefaultTenantProfileSettings _defaultSettings;

    public UpdateRegionalSettingsHandler(IApplicationDbContext context, IOptions<DefaultTenantProfileSettings> defaultSettings)
    {
        _context = context;
        _defaultSettings = defaultSettings.Value;
    }

    public async Task<Result> Handle(UpdateRegionalSettingsCommand request, CancellationToken cancellationToken)
    {
        var profile = await _context.TenantProfiles.FirstOrDefaultAsync(cancellationToken);

        if (profile == null)
        {
            profile = new Onyx.Oms.Core.Domain.Entities.TenantProfile(
                Guid.NewGuid(),
                _defaultSettings.StoreName,
                _defaultSettings.ContactEmail,
                string.IsNullOrWhiteSpace(_defaultSettings.BaseCurrency) ? "LKR" : _defaultSettings.BaseCurrency,
                string.IsNullOrWhiteSpace(_defaultSettings.WeightUnit) ? "kg" : _defaultSettings.WeightUnit
            );
            _context.TenantProfiles.Add(profile);
        }

        profile.UpdateRegionalSettings(request.BaseCurrency, request.WeightUnit);

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
