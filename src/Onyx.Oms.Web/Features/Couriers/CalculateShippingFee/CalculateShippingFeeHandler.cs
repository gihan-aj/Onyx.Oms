using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Couriers.CalculateShippingFee
{
    public class CalculateShippingFeeHandler : IQueryHandler<CalculateShippingFeeQuery, decimal>
    {
        private readonly IApplicationDbContext _context;

        public CalculateShippingFeeHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<decimal>> Handle(CalculateShippingFeeQuery request, CancellationToken cancellationToken)
        {
            var courier = await _context.Couriers
                .Include(c => c.ZoneRates)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == request.CourierId, cancellationToken);

            if (courier == null)
                return Result.Failure<decimal>(Error.NotFound("Courier.NotFound", "Courier not found."));

            var zoneRate = courier.GetApplicableRate(request.District);
            if (zoneRate == null)
                return Result.Failure<decimal>(Error.NotFound("ZoneRate.NotFound", "A Zone Rate is not found to calculate the Shipping Fee."));

            var fee = zoneRate.CalculateShippingFee(request.TotalWeightKg, request.CodAmount);

            return fee;
        }
    }
}