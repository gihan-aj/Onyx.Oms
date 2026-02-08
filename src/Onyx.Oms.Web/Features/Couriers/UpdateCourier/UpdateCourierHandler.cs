using MediatR;
using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Models;

namespace Onyx.Oms.Web.Features.Couriers.UpdateCourier;

public class UpdateCourierHandler : IRequestHandler<UpdateCourierCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public UpdateCourierHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(UpdateCourierCommand request, CancellationToken cancellationToken)
    {
        var courier = await _context.Couriers
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (courier is null)
        {
            return Result.Failure(Error.NotFound("Courier.NotFound", "Courier not found."));
        }

        if (courier.Name != request.Name)
        {
            bool isNameUnique = !await _context.Couriers
                .AnyAsync(c => c.Name == request.Name && c.Id != request.Id, cancellationToken);

            if (!isNameUnique)
            {
                return Result.Failure(Error.Conflict("Courier.NameNotUnique", "A courier with this name already exists."));
            }
        }

        courier.UpdateDetails(
            request.Name,
            request.ContactPerson,
            request.PrimaryPhone,
            request.SecondaryPhone,
            request.WebsiteUrl,
            request.TrackingUrlTemplate);

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
