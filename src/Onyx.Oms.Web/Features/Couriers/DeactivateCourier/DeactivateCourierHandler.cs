using MediatR;
using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Models;

namespace Onyx.Oms.Web.Features.Couriers.DeactivateCourier;

public class DeactivateCourierHandler : IRequestHandler<DeactivateCourierCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public DeactivateCourierHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(DeactivateCourierCommand request, CancellationToken cancellationToken)
    {
        var courier = await _context.Couriers
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (courier is null)
        {
            return Result.Failure(Error.NotFound("Courier.NotFound", "Courier not found."));
        }

        if (!courier.IsActive)
        {
            return Result.Success(); 
        }

        courier.Deactivate();
        
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
