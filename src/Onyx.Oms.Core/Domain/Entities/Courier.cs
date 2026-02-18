using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Models;

namespace Onyx.Oms.Core.Domain.Entities;

public class Courier : AuditableEntity<Guid>
{
    public string Name { get; private set; } = string.Empty;
    public string? ContactPerson { get; private set; }
    public string? PrimaryPhone { get; private set; }
    public string? SecondaryPhone { get; private set; }
    public string? WebsiteUrl { get; private set; }
    public string? TrackingUrlTemplate { get; private set; }
    public bool IsActive { get; private set; }

    // EF Core Constructor
    private Courier() { }

    private Courier(
        Guid id,
        string name,
        string? contactPerson,
        string? primaryPhone,
        string? secondaryPhone,
        string? websiteUrl,
        string? trackingUrlTemplate,
        bool isActive) : base(id)
    {
        Name = name;
        ContactPerson = contactPerson;
        PrimaryPhone = primaryPhone;
        SecondaryPhone = secondaryPhone;
        WebsiteUrl = websiteUrl;
        TrackingUrlTemplate = trackingUrlTemplate;
        IsActive = isActive;
    }

    public static Result<Courier> Create(
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
            Guid.NewGuid(),
            name,
            contactPerson,
            primaryPhone,
            secondaryPhone,
            websiteUrl,
            trackingUrlTemplate,
            isActive: true);

        return Result.Success(courier);
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

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}
