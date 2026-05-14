using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Domain.ValueObjects;

namespace Onyx.Oms.Core.Domain.Entities;

public class Customer : AuditableEntity<Guid>, IMustHaveTenant
{
    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Email { get; private set; }
    public string PrimaryPhone { get; private set; } = string.Empty;
    public string? SecondaryPhone { get; private set; }
    public Address Address { get; private set; } = Address.Empty;
    public string? LastOrderNumber { get; private set; }
    public string? Notes { get; private set; }
    public string? DeliveryInstructions { get; private set; }
    public bool IsActive { get; private set; }

    // Private constructor for EF Core
    private Customer(): base(Guid.NewGuid()) { }

    private Customer(Guid tenantId, string name, string? email, string primaryPhone, string? secondaryPhone, Address? address, string? notes, string? deliveryInstructions)
        : base(Guid.NewGuid())
    {
        TenantId = tenantId;
        Name = name;
        Email = string.IsNullOrWhiteSpace(email) ? null : email;
        PrimaryPhone = primaryPhone;
        SecondaryPhone = string.IsNullOrWhiteSpace(secondaryPhone) ? null : secondaryPhone;
        Address = address ?? Address.Empty;
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes;
        DeliveryInstructions = string.IsNullOrWhiteSpace(deliveryInstructions) ? null : deliveryInstructions;
        IsActive = true;
    }

    public static Result<Customer> Create(
        Guid tenantId,
        string name,
        string? email,
        string primaryPhone,
        string? secondaryPhone,
        Address? address,
        string? notes,
        string? deliveryInstructions)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<Customer>(Error.Validation("Customer.NameRequired", "Name is required."));

        if (string.IsNullOrWhiteSpace(primaryPhone))
            return Result.Failure<Customer>(Error.Validation("Customer.PrimaryPhoneRequired", "Primary Phone is required."));

        var customer = new Customer(tenantId, name, email, primaryPhone, secondaryPhone, address, notes, deliveryInstructions);

        return customer;
    }

    public void UpdateDetails(
        string name,
        string? email,
        string primaryPhone,
        string? secondaryPhone,
        Address? address,
        string? notes,
        string? deliveryInstructions)
    {
        Name = name;
        Email = email;
        PrimaryPhone = primaryPhone;
        SecondaryPhone = secondaryPhone;
        Address = address ?? Address.Empty;
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes;
        DeliveryInstructions = string.IsNullOrWhiteSpace(deliveryInstructions) ? null : deliveryInstructions;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
    public void UpdateLastOrder(string orderNumber) => LastOrderNumber = orderNumber;
}
