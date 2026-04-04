using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Entities;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Couriers.CreateCourier;

public class CreateCourierHandler : ICommandHandler<CreateCourierCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public CreateCourierHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<Guid>> Handle(CreateCourierCommand request, CancellationToken cancellationToken)
    {
        bool courierExists = await _context.Couriers
            .AnyAsync(c => c.Name == request.Name, cancellationToken);

        if (courierExists)
            return Result.Failure<Guid>(Error.Conflict("Courier.NameExists", "A courier with the same name already exists."));

        Guid? tenantId = _currentUserService.ActiveTenantId;
        if (tenantId == null)
            return Result.Failure<Guid>(Error.Unauthorized("Courier.TenantIdMissing", "Tenant Id not found."));

        var courierResult = Courier.Create(
            tenantId.Value,
            request.Name,
            request.ContactPerson,
            request.PrimaryPhone,
            request.SecondaryPhone,
            request.WebsiteUrl,
            request.TrackingUrlTemplate);

        if (courierResult.IsFailure)
        {
            return Result.Failure<Guid>(courierResult.Error);
        }
        
        _context.Couriers.Add(courierResult.Value);

        await _context.SaveChangesAsync(cancellationToken);

        return courierResult.Value.Id;
    }
}
