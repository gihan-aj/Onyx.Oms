using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Messaging;
using Onyx.Oms.Web.Features.Settings.TenantProfile.GetProfile;

namespace Onyx.Oms.Web.Features.Settings.TenantProfile.UpdateLogo
{
    public class UpdateTenantLogoHandler : ICommandHandler<UpdateTenantLogoCommand>
    {
        private readonly IApplicationDbContext _context;

        public UpdateTenantLogoHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result> Handle(UpdateTenantLogoCommand request, CancellationToken cancellationToken)
        {
            var profile = await _context.Tenants.FirstOrDefaultAsync(cancellationToken);

            if (profile == null)
            {
                return Result.Failure<TenantProfileDto>(Error.NotFound("Tenant.NotFound", "Tenant details not found."));
            }

            profile.UpdateLogo(request.LogoUrl);

            await _context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
    }
}
