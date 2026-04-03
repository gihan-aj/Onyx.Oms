using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Messaging;
using Onyx.Oms.Web.Features.Settings.TenantProfile.GetProfile;

namespace Onyx.Oms.Web.Features.Settings.TenantProfile.UpdateHeroImage
{
    public class UpdateTenantHeroImageHandler : ICommandHandler<UpdateTenantHeroImageCommand>
    {
        private readonly IApplicationDbContext _context;
        public UpdateTenantHeroImageHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result> Handle(UpdateTenantHeroImageCommand request, CancellationToken cancellationToken)
        {
            var profile = await _context.Tenants.FirstOrDefaultAsync(cancellationToken);

            if (profile == null)
            {
                return Result.Failure<TenantProfileDto>(Error.NotFound("Tenant.NotFound", "Tenant details not found."));
            }

            profile.UpdateHeroImage(request.HeroImageUrl);

            await _context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
    }
}
