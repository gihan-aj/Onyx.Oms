using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Events;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Domain.ValueObjects;

namespace Onyx.Oms.Core.Domain.Entities
{
    public class Tenant : AuditableEntity<Guid>
    {
        private readonly List<AppUser> _users = new();

        private Tenant() { }

        private Tenant(Guid? id, string companyName, string contactEmail, string? contactPhone) : base(id ?? Guid.NewGuid())
        {
            CompanyName = companyName;
            ContactEmail = contactEmail;
            if(!string.IsNullOrWhiteSpace(contactEmail))
                ContactPhone = contactPhone;

            IsActive = true;
        }

        // Identity & Contact
        public string CompanyName { get; private set; } = string.Empty;
        public string ContactEmail { get; private set; } = string.Empty;
        public string? ContactPhone { get; private set; }
        public string? LegalName { get; private set; }
        public string? TaxRegistrationNumber { get; private set; }

        // Physical Location
        public Address? StoreAddress { get; private set; }

        // Regional & Units
        public string DefaultCurrency { get; private set; } = "LKR";
        public string TimeZone { get; private set; } = "UTC";
        public string WeightUnit { get; private set; } = "kg";

        // Invoicing & Documents
        public string? InvoiceFooterText { get; private set; }
        public string? LogoUrl { get; private set; }
        public string? HeroImageUrl {  get; private set; }

        // Loose UI Preferences (Serialized JSON)
        public string PreferencesJson { get; private set; } = "{}";

        public TenantSubscription Subscription { get; private set; } = null!;

        public IReadOnlyCollection<AppUser> Users => _users.AsReadOnly();

        public bool IsActive { get; private set; }

        public static Result<Tenant> Create(string companyName, string contactEmail, string? contactPhone, Guid? explicitId = null)
        {
            if (string.IsNullOrWhiteSpace(companyName))
                return Result.Failure<Tenant>(Error.Validation("Tenant.ComanyNameRequired", "Company Name is required."));

            if (string.IsNullOrWhiteSpace(contactEmail))
                return Result.Failure<Tenant>(Error.Validation("Tenant.EmailRequired", "Email is required."));

            var tenant = new Tenant(explicitId, companyName, contactEmail, contactPhone);

            //tenant.RaiseDomainEvent(new TenantCreatedDomainEvent(tenant.Id));

            return tenant;
        }

        // --- Domain Behaviors ---
        public Result UpdateStoreInfo(string companyName, string? legalName, string? taxId, string contactEmail, string? phone, string? invoiceFooterText)
        {
            if (string.IsNullOrWhiteSpace(companyName))
                return Result.Failure(Error.Validation("Tenant.CompanyNameRequired", "Company Name is required."));

            if (string.IsNullOrWhiteSpace(contactEmail))
                return Result.Failure(Error.Validation("Tenant.ContactEmailRequired", "Contact Email is required."));

            CompanyName = companyName;
            LegalName = legalName;
            TaxRegistrationNumber = taxId;
            ContactEmail = contactEmail;
            ContactPhone = phone;
            InvoiceFooterText = string.IsNullOrWhiteSpace(invoiceFooterText) ? null : invoiceFooterText;

            return Result.Success();
        }

        public Result UpdateRegionalSettings(string currency, string timeZone, string weightUnit)
        {
            if (string.IsNullOrWhiteSpace(currency) || string.IsNullOrWhiteSpace(timeZone) || string.IsNullOrWhiteSpace(weightUnit))
                return Result.Failure(Error.Validation("Tenant.RegionalSettingsRequired", "Currency, Time Zone and Weight Unit are required."));

            DefaultCurrency = currency.ToUpperInvariant();
            WeightUnit = weightUnit.ToLowerInvariant();

            return Result.Success();
        }

        public Result UpdateAddress(Address address)
        {
            if (address == null)
                return Result.Failure(Error.Validation("Tenant.AddressRequired", "Address is required."));

            StoreAddress = address;
            return Result.Success();
        }

        public void UpdateLogo(string? logoUrl)
        {
            if (string.IsNullOrWhiteSpace(logoUrl))
                LogoUrl = null;
            else
                LogoUrl = logoUrl;
        }

        public void UpdateHeroImage(string? heroImageUrl)
        {
            if(string.IsNullOrWhiteSpace(heroImageUrl))
                HeroImageUrl = null;
            else
                HeroImageUrl = heroImageUrl;
        }

        public Result UpdatePreferences(string jsonFormattedPreferences)
        {
            PreferencesJson = jsonFormattedPreferences;
            return Result.Success();
        }

        public Result SetSubscription(TenantSubscription subscription)
        {
            if (subscription == null)
                return Result.Failure(Error.Validation("Tenant.SubscriptionRequired", "Subscription is required."));

            Subscription = subscription;
            return Result.Success();
        }

        public Result AddUser(AppUser user)
        {
            if (user == null)
                return Result.Failure(Error.Validation("Tenant.UserRequired", "User is required."));

            if (!_users.Contains(user))
            {
                _users.Add(user);
            }

            return Result.Success();
        }
    }
}
