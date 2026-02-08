using MediatR;
using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Models;

namespace Onyx.Oms.Web.Features.Couriers.DeleteCourier;

public class DeleteCourierHandler : IRequestHandler<DeleteCourierCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public DeleteCourierHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(DeleteCourierCommand request, CancellationToken cancellationToken)
    {
        var courier = await _context.Couriers
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (courier is null)
        {
            return Result.Failure(Error.NotFound("Courier.NotFound", "Courier not found."));
        }

        _context.Couriers.Remove(courier);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
