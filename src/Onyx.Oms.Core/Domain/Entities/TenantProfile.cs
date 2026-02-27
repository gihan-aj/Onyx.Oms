using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.ValueObjects;

namespace Onyx.Oms.Core.Domain.Entities;

public class TenantProfile : AuditableEntity<Guid>
{
    private TenantProfile() { } // EF Core requirement

    public TenantProfile(
        Guid id, 
        string storeName, 
        string contactEmail, 
        string baseCurrency = "LKR", 
        string weightUnit = "kg") : base(id)
    {
        StoreName = storeName;
        ContactEmail = contactEmail;
        BaseCurrency = baseCurrency.ToUpperInvariant();
        WeightUnit = weightUnit.ToLowerInvariant();
        PreferencesJson = "{}"; 
    }

    // Identity & Contact
    public string StoreName { get; private set; } = string.Empty;
    public string? LegalName { get; private set; }
    public string? TaxRegistrationNumber { get; private set; }
    public string ContactEmail { get; private set; } = string.Empty;
    public string? ContactPhone { get; private set; }

    // Physical Location
    public Address? StoreAddress { get; private set; }

    // Regional & Units (The Single Source of Truth)
    public string BaseCurrency { get; private set; } = "LKR";
    public string WeightUnit { get; private set; } = "kg";

    // Invoicing & Documents
    public string? InvoiceFooterText { get; private set; }
    public string? LogoUrl { get; private set; }

    // Loose UI Preferences (Serialized JSON)
    public string PreferencesJson { get; private set; } = "{}";

    // --- Domain Behaviors ---

    public void UpdateStoreInfo(string storeName, string? legalName, string? taxId, string contactEmail, string? phone)
    {
        if (string.IsNullOrWhiteSpace(storeName)) 
            throw new ArgumentException("Store Name is required.");

        if (string.IsNullOrWhiteSpace(contactEmail)) 
            throw new ArgumentException("Contact Email is required.");

        StoreName = storeName;
        LegalName = legalName;
        TaxRegistrationNumber = taxId;
        ContactEmail = contactEmail;
        ContactPhone = phone;
    }

    public void UpdateRegionalSettings(string currency, string weightUnit)
    {
        if (string.IsNullOrWhiteSpace(currency) || string.IsNullOrWhiteSpace(weightUnit))
            throw new ArgumentException("Currency and Weight Unit are required.");

        BaseCurrency = currency.ToUpperInvariant();
        WeightUnit = weightUnit.ToLowerInvariant();
    }

    public void UpdateAddress(Address address) => StoreAddress = address;

    public void UpdatePreferences(string jsonFormattedPreferences)
    {
        PreferencesJson = jsonFormattedPreferences;
    }
}
