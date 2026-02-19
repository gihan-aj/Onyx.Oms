using MediatR;
using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Couriers.GetCouriers;

public class GetCouriersHandler : IQueryHandler<GetCouriersQuery, IEnumerable<CourierDto>>
{
    private readonly IApplicationDbContext _context;

    public GetCouriersHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IEnumerable<CourierDto>>> Handle(GetCouriersQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Couriers.AsNoTracking();

        if (request.IsActive.HasValue)
        {
            query = query.Where(c => c.IsActive == request.IsActive.Value);
        }

        var couriers = await query
            .OrderBy(c => c.Name)
            .Select(c => new CourierDto(
                c.Id,
                c.Name,
                c.ContactPerson,
                c.PrimaryPhone,
                c.SecondaryPhone,
                c.WebsiteUrl,
                c.IsActive))
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<CourierDto>>(couriers);
    }
}
