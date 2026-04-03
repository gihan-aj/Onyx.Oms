using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Settings.TenantProfile.GetProfile;

public class GetTenantProfileHandler : IQueryHandler<GetTenantProfileQuery, TenantProfileDto>
{
    private readonly IApplicationDbContext _context;

    public GetTenantProfileHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<TenantProfileDto>> Handle(GetTenantProfileQuery request, CancellationToken cancellationToken)
    {
        var profile = await _context.Tenants.FirstOrDefaultAsync(cancellationToken);

        if (profile == null)
        {
            return Result.Failure<TenantProfileDto>(Error.NotFound("Tenant.NotFound", "Tenant details not found."));
        }

        var dto = new TenantProfileDto(
            profile.Id,
            profile.CompanyName,
            profile.ContactEmail,
            profile.ContactPhone,
            profile.LegalName,
            profile.TaxRegistrationNumber,
            profile.StoreAddress,
            profile.DefaultCurrency,
            profile.WeightUnit,
            profile.InvoiceFooterText,
            profile.LogoUrl,
            profile.HeroImageUrl,
            profile.PreferencesJson
        );

        return Result<TenantProfileDto>.Success(dto);
    }
}
