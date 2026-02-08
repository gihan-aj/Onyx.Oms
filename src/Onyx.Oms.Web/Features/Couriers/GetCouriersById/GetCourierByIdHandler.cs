using MediatR;
using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Models;

namespace Onyx.Oms.Web.Features.Couriers.GetCouriersById;

public class GetCourierByIdHandler : IRequestHandler<GetCourierByIdQuery, Result<CourierDto>>
{
    private readonly IApplicationDbContext _context;

    public GetCourierByIdHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<CourierDto>> Handle(GetCourierByIdQuery request, CancellationToken cancellationToken)
    {
        var courier = await _context.Couriers
            .AsNoTracking()
            .Where(c => c.Id == request.Id)
            .Select(c => new CourierDto(
                c.Id,
                c.Name,
                c.ContactPerson,
                c.PrimaryPhone,
                c.SecondaryPhone,
                c.WebsiteUrl,
                c.TrackingUrlTemplate,
                c.IsActive))
            .FirstOrDefaultAsync(cancellationToken);

        if (courier is null)
        {
            return Result.Failure<CourierDto>(Error.NotFound("Courier.NotFound", $"Courier with ID {request.Id} not found."));
        }

        return Result.Success(courier);
    }
}
