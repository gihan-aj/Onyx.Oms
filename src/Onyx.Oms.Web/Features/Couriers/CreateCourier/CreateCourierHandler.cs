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
            return Result.Failure<Guid>(courierResult.Error);

        var courier = courierResult.Value;

        // Determine which zone rates to seed
        var zonesToAdd = request.ZoneRates != null && request.ZoneRates.Any()
            ? request.ZoneRates
            : GetDefaultZoneRates();

        foreach (var dto in zonesToAdd)
        {
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
                return Result.Failure<Guid>(addResult.Error);

            _context.CourierZoneRates.Add(addResult.Value);
        }

        _context.Couriers.Add(courier);

        await _context.SaveChangesAsync(cancellationToken);

        return courier.Id;
    }

    /// <summary>
    /// Produces the two auto-seeded zones described in the UI/UX guidelines:
    /// a "Colombo" zone covering the Colombo district and an "Outstation" default fallback.
    /// </summary>
    private static List<CreateCourierZoneRateDto> GetDefaultZoneRates() =>
    [
        new CreateCourierZoneRateDto(
            ZoneName: "Colombo",
            BaseFee: 0,
            BaseWeight: 1,
            ExcessFeePerWeightUnit: 0,
            CodPercentage: 0,
            Currency: "LKR",
            WeightUnit: "kg",
            IsDefault: false,
            CoveredDistricts: ["Colombo"]),

        new CreateCourierZoneRateDto(
            ZoneName: "Outstation",
            BaseFee: 0,
            BaseWeight: 1,
            ExcessFeePerWeightUnit: 0,
            CodPercentage: 0,
            Currency: "LKR",
            WeightUnit: "kg",
            IsDefault: true,
            CoveredDistricts: []),
    ];
}
