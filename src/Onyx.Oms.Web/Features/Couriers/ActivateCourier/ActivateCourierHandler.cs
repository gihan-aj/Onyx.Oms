using MediatR;
using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Models;

namespace Onyx.Oms.Web.Features.Couriers.ActivateCourier;

public class ActivateCourierHandler : IRequestHandler<ActivateCourierCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public ActivateCourierHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(ActivateCourierCommand request, CancellationToken cancellationToken)
    {
        var courier = await _context.Couriers
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (courier is null)
        {
            return Result.Failure(Error.NotFound("Courier.NotFound", "Courier not found."));
        }

        if (courier.IsActive)
        {
            return Result.Success();
        }
            
        courier.Activate();
        
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
