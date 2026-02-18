using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Domain.ValueObjects;

namespace Onyx.Oms.Core.Domain.Entities;

public class Customer : AuditableEntity<Guid>
{
    public string Name { get; private set; } = string.Empty;
    public string? Email { get; private set; }
    public string PrimaryPhone { get; private set; } = string.Empty;
    public string? SecondaryPhone { get; private set; }
    public Address Address { get; private set; } = Address.Empty;
    public string? Notes { get; private set; }
    public bool IsActive { get; private set; }

    // Private constructor for EF Core
    private Customer() { }

    public static Result<Customer> Create(
        string name,
        string? email,
        string primaryPhone,
        string? secondaryPhone,
        Address? address,
        string? notes)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<Customer>(Error.Validation("Customer.NameRequired", "Name is required."));

        if (string.IsNullOrWhiteSpace(primaryPhone))
            return Result.Failure<Customer>(Error.Validation("Customer.PrimaryPhoneRequired", "Primary Phone is required."));

        return Result.Success(new Customer
        {
            Id = Guid.NewGuid(),
            Name = name,
            Email = email,
            PrimaryPhone = primaryPhone,
            SecondaryPhone = secondaryPhone,
            Address = address ?? Address.Empty,
            Notes = notes,
            IsActive = true
        });
    }

    public void UpdateDetails(
        string name,
        string? email,
        string primaryPhone,
        string? secondaryPhone,
        Address? address,
        string? notes)
    {
        Name = name;
        Email = email;
        PrimaryPhone = primaryPhone;
        SecondaryPhone = secondaryPhone;
        Address = address ?? Address.Empty;
        Notes = notes;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}
