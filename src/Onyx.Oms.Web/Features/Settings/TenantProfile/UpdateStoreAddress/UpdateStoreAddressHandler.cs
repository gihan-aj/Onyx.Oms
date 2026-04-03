using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Messaging;
using Onyx.Oms.Web.Features.Settings.TenantProfile.GetProfile;

namespace Onyx.Oms.Web.Features.Settings.TenantProfile.UpdateStoreAddress;

public class UpdateStoreAddressHandler : ICommandHandler<UpdateStoreAddressCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateStoreAddressHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(UpdateStoreAddressCommand request, CancellationToken cancellationToken)
    {
        var profile = await _context.Tenants.FirstOrDefaultAsync(cancellationToken);

        if (profile == null)
        {
            return Result.Failure<TenantProfileDto>(Error.NotFound("Tenant.NotFound", "Tenant details not found."));
        }

        profile.UpdateAddress(request.StoreAddress);

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
