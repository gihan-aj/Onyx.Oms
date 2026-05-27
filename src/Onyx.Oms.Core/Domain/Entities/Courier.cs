using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Models;

namespace Onyx.Oms.Core.Domain.Entities;

public class Courier : AuditableEntity<Guid>, IMustHaveTenant
{
    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? ContactPerson { get; private set; }
    public string? PrimaryPhone { get; private set; }
    public string? SecondaryPhone { get; private set; }
    public string? WebsiteUrl { get; private set; }
    public string? TrackingUrlTemplate { get; private set; }
    public bool IsActive { get; private set; }

    private readonly List<CourierZoneRate> _zoneRates = new();
    public IReadOnlyCollection<CourierZoneRate> ZoneRates => _zoneRates.AsReadOnly();

    // EF Core Constructor
    private Courier() { }

    private Courier(
        Guid tenantId,
        string name,
        string? contactPerson,
        string? primaryPhone,
        string? secondaryPhone,
        string? websiteUrl,
        string? trackingUrlTemplate,
        bool isActive) : base(Guid.NewGuid())
    {
        TenantId = tenantId;
        Name = name;
        ContactPerson = contactPerson;
        PrimaryPhone = primaryPhone;
        SecondaryPhone = secondaryPhone;
        WebsiteUrl = websiteUrl;
        TrackingUrlTemplate = trackingUrlTemplate;
        IsActive = isActive;
    }

    public static Result<Courier> Create(
        Guid tenantId,
        string name,
        string? contactPerson,
        string? primaryPhone,
        string? secondaryPhone,
        string? websiteUrl,
        string? trackingUrlTemplate)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<Courier>(Error.Validation("Courier.NameEmpty", "Courier name cannot be empty."));
        }

        var courier = new Courier(
            tenantId,
            name,
            contactPerson,
            primaryPhone,
            secondaryPhone,
            websiteUrl,
            trackingUrlTemplate,
            isActive: true);

        return Result.Success(courier);
    }

    public Result<CourierZoneRate> AddZoneRate(
        string zoneName,
        decimal baseFee,
        decimal baseWeight,
        decimal excessFeePerWeightUnit,
        decimal codPercentage,
        string currency,
        string weightUnit,
        bool isDefault,
        List<string> coveredDistricts)
    {
        var createResult = CourierZoneRate.Create(
            TenantId,
            Id,
            zoneName,
            baseFee,
            baseWeight,
            excessFeePerWeightUnit,
            codPercentage,
            currency,
            weightUnit,
            isDefault,
            coveredDistricts);

        if (createResult.IsFailure)
            return Result.Failure<CourierZoneRate>(createResult.Error);

        var zoneRate = createResult.Value;

        if (isDefault && _zoneRates.Any(zr => zr.IsDefault))
         return Result.Failure<CourierZoneRate>(Error.Conflict("CourierZoneRate.DefaultConflict", "Only one default zone rate is allowed per courier."));

        _zoneRates.Add(zoneRate);
        return Result.Success(zoneRate);
    }

    public Result RemoveZoneRate(Guid zoneRateId)
    {
        var zoneRate = _zoneRates.FirstOrDefault(zr => zr.Id == zoneRateId);

        if (zoneRate == null)
            return Result.Failure(Error.NotFound("CourierZoneRate.NotFound", "Zone rate not found."));

        _zoneRates.Remove(zoneRate);
        return Result.Success();
    }

    public void UpdateDetails(
        string name,
        string? contactPerson,
        string? primaryPhone,
        string? secondaryPhone,
        string? websiteUrl,
        string? trackingUrlTemplate)
    {
        // Add business logic/validation here if needed
        Name = name;
        ContactPerson = contactPerson;
        PrimaryPhone = primaryPhone;
        SecondaryPhone = secondaryPhone;
        WebsiteUrl = websiteUrl;
        TrackingUrlTemplate = trackingUrlTemplate;
    }

    public Result UpdateZoneRate(
        Guid zoneRateId,
        string zoneName,
        decimal baseFee,
        decimal baseWeight,
        decimal excessFeePerWeightUnit,
        decimal codPercentage,
        string currency,
        string weightUnit,
        bool isDefault,
        List<string> coveredDistricts)
    {
        var zoneRate = _zoneRates.FirstOrDefault(zr => zr.Id == zoneRateId);

        if (zoneRate == null)
            return Result.Failure(Error.NotFound("CourierZoneRate.NotFound", "Zone rate not found."));

        if (isDefault && _zoneRates.Any(zr => zr.IsDefault && zr.Id != zoneRateId))
            return Result.Failure<CourierZoneRate>(Error.Conflict("CourierZoneRate.DefaultConflict", "Only one default zone rate is allowed per courier."));

        var updateResult = zoneRate.Update(
            zoneName,
            baseFee,
            baseWeight,
            excessFeePerWeightUnit,
            codPercentage,
            currency,
            weightUnit,
            isDefault,
            coveredDistricts);

        if (updateResult.IsFailure)
            return updateResult;

        return Result.Success();
    }

    public CourierZoneRate? GetApplicableRate(string targetDistrict)
    {
        if (string.IsNullOrWhiteSpace(targetDistrict) || ZoneRates.Count == 0)
            return null;

        var specificZone = ZoneRates
            .FirstOrDefault(z => !z.IsDefault && z.CoveredDistrics.Contains(targetDistrict, StringComparer.OrdinalIgnoreCase));

        if(specificZone != null)
            return specificZone;

        var defaultZone = ZoneRates.FirstOrDefault(z => z.IsDefault);
        if (defaultZone != null) 
            return defaultZone;

        return null;
    }

    public CourierZoneRate? GetDefaultZoneRate()
    {
        return _zoneRates.FirstOrDefault(zr => zr.IsDefault);
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}
