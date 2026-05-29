using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Couriers.UpdateCourier;

public class UpdateCourierHandler : ICommandHandler<UpdateCourierCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateCourierHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(UpdateCourierCommand request, CancellationToken cancellationToken)
    {
        var courier = await _context.Couriers
            .Include(c => c.ZoneRates)
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (courier is null)
            return Result.Failure(Error.NotFound("Courier.NotFound", "Courier not found."));

        if (courier.Name != request.Name)
        {
            if(courier.IsSystemManaged)
                return Result.Failure(Error.Conflict("Courier.SystemManaged", "You cannot change the name for system managed couriers."));

            bool isNameUnique = !await _context.Couriers
                .AnyAsync(c => c.Name == request.Name && c.Id != request.Id, cancellationToken);

            if (!isNameUnique)
                return Result.Failure(Error.Conflict("Courier.NameNotUnique", "A courier with this name already exists."));
        }

        courier.UpdateDetails(
            request.Name,
            request.ContactPerson,
            request.PrimaryPhone,
            request.SecondaryPhone,
            request.WebsiteUrl,
            request.TrackingUrlTemplate);

        if (request.ZoneRates != null)
        {
            var incomingIds = request.ZoneRates
                .Where(z => z.Id.HasValue && z.Id.Value != Guid.Empty)
                .Select(z => z.Id!.Value)
                .ToHashSet();

            // 1. Remove zone rates that are no longer in the request
            var toRemove = courier.ZoneRates
                .Where(zr => !incomingIds.Contains(zr.Id))
                .ToList();

            foreach (var zoneRate in toRemove)
            {
                var removeResult = courier.RemoveZoneRate(zoneRate.Id);
                if (removeResult.IsFailure)
                    return removeResult;

                _context.CourierZoneRates.Remove(zoneRate);
            }

            // 2. Update existing / add new zone rates
            foreach (var dto in request.ZoneRates)
            {
                bool isExisting = dto.Id.HasValue && dto.Id.Value != Guid.Empty;

                if (isExisting)
                {
                    // Update in-place via domain method
                    var updateResult = courier.UpdateZoneRate(
                        dto.Id!.Value,
                        dto.ZoneName,
                        dto.BaseFee,
                        dto.BaseWeight,
                        dto.ExcessFeePerWeightUnit,
                        dto.CodPercentage,
                        dto.Currency,
                        dto.WeightUnit,
                        dto.IsDefault,
                        dto.CoveredDistricts);

                    if (updateResult.IsFailure)
                        return updateResult;
                }
                else
                {
                    // New zone rate
                    var addResult = courier.AddZoneRate(
                        dto.ZoneName,
                        dto.BaseFee,
                        dto.BaseWeight,
                        dto.ExcessFeePerWeightUnit,
                        dto.CodPercentage,
                        dto.Currency,
                        dto.WeightUnit,
                        dto.IsDefault,
                        dto.CoveredDistricts);

                    if (addResult.IsFailure)
                        return Result.Failure(addResult.Error);

                    _context.CourierZoneRates.Add(addResult.Value);
                }
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
