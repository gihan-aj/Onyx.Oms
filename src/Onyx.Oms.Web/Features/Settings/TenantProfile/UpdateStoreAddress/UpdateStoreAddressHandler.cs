using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Messaging;
using Onyx.Oms.Web.Common.Settings;

namespace Onyx.Oms.Web.Features.Settings.TenantProfile.UpdateStoreAddress;

public class UpdateStoreAddressHandler : ICommandHandler<UpdateStoreAddressCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly DefaultTenantProfileSettings _defaultSettings;

    public UpdateStoreAddressHandler(IApplicationDbContext context, IOptions<DefaultTenantProfileSettings> defaultSettings)
    {
        _context = context;
        _defaultSettings = defaultSettings.Value;
    }

    public async Task<Result> Handle(UpdateStoreAddressCommand request, CancellationToken cancellationToken)
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

        profile.UpdateAddress(request.StoreAddress);

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
