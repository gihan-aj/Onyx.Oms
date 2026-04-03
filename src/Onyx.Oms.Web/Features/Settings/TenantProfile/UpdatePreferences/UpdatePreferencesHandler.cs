using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Messaging;
using Onyx.Oms.Web.Common.Settings;
using Onyx.Oms.Web.Features.Settings.TenantProfile.GetProfile;

namespace Onyx.Oms.Web.Features.Settings.TenantProfile.UpdatePreferences;

public class UpdatePreferencesHandler : ICommandHandler<UpdatePreferencesCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly DefaultTenantProfileSettings _defaultSettings;

    public UpdatePreferencesHandler(IApplicationDbContext context, IOptions<DefaultTenantProfileSettings> defaultSettings)
    {
        _context = context;
        _defaultSettings = defaultSettings.Value;
    }

    public async Task<Result> Handle(UpdatePreferencesCommand request, CancellationToken cancellationToken)
    {
        var profile = await _context.Tenants.FirstOrDefaultAsync(cancellationToken);

        if (profile == null)
        {
            return Result.Failure<TenantProfileDto>(Error.NotFound("Tenant.NotFound", "Tenant details not found."));
        }

        profile.UpdatePreferences(request.PreferencesJson);

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
