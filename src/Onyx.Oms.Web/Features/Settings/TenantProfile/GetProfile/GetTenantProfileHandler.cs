using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Messaging;
using Onyx.Oms.Web.Common.Settings;

namespace Onyx.Oms.Web.Features.Settings.TenantProfile.GetProfile;

public class GetTenantProfileHandler : IQueryHandler<GetTenantProfileQuery, TenantProfileDto>
{
    private readonly IApplicationDbContext _context;
    private readonly DefaultTenantProfileSettings _defaultSettings;

    public GetTenantProfileHandler(IApplicationDbContext context, IOptions<DefaultTenantProfileSettings> defaultSettings)
    {
        _context = context;
        _defaultSettings = defaultSettings.Value;
    }

    public async Task<Result<TenantProfileDto>> Handle(GetTenantProfileQuery request, CancellationToken cancellationToken)
    {
        var profile = await _context.TenantProfiles.FirstOrDefaultAsync(cancellationToken);

        if (profile == null)
        {
            // Initialize default profile
            profile = new Onyx.Oms.Core.Domain.Entities.TenantProfile(
                Guid.NewGuid(),
                _defaultSettings.StoreName,
                _defaultSettings.ContactEmail,
                string.IsNullOrWhiteSpace(_defaultSettings.BaseCurrency) ? "LKR" : _defaultSettings.BaseCurrency,
                string.IsNullOrWhiteSpace(_defaultSettings.WeightUnit) ? "kg" : _defaultSettings.WeightUnit
            );

            _context.TenantProfiles.Add(profile);
            await _context.SaveChangesAsync(cancellationToken);
        }

        var dto = new TenantProfileDto(
            profile.Id,
            profile.StoreName,
            profile.LegalName,
            profile.TaxRegistrationNumber,
            profile.ContactEmail,
            profile.ContactPhone,
            profile.StoreAddress,
            profile.BaseCurrency,
            profile.WeightUnit,
            profile.InvoiceFooterText,
            profile.LogoUrl,
            profile.PreferencesJson
        );

        return Result<TenantProfileDto>.Success(dto);
    }
}
