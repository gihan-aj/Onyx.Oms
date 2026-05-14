using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Entities;
using Onyx.Oms.Core.Domain.Enums;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Couriers.DeactivateCourier;

public class DeactivateCourierHandler : ICommandHandler<DeactivateCourierCommand>
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

        var processingOrdersExists = await _context.Orders
            .AnyAsync(o =>
                o.CourierId == courier.Id &&
                (o.Status == OrderStatus.Pending ||
                o.Status == OrderStatus.Confirmed ||
                o.Status == OrderStatus.Processing ||
                o.Status == OrderStatus.ReadyToPack ||
                o.Status == OrderStatus.Packed ||
                o.Status == OrderStatus.Shipped ||
                o.Status == OrderStatus.Delivered),
                cancellationToken);
        if (processingOrdersExists)
        {
            return Result.Failure(Error.Validation("Courier.HasUnfinishedOrders", "Courier has unfinished orders and cannot be deactivated."));
        }

        courier.Deactivate();
        
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
