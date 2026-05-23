using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Models;

namespace Onyx.Oms.Core.Domain.Entities
{
    public class TenantWhatsAppSettings : AuditableEntity<Guid>
    {
        public Guid TenantId { get; private set; }
        public string PhoneNumberId { get; private set; } = string.Empty;
        public string EncryptedAccessToken { get; private set; } = string.Empty;
        public bool IsActive { get; private set; }

        private TenantWhatsAppSettings() { }

        private TenantWhatsAppSettings(Guid tenantId, string phoneNumberId, string encryptedAccessToken) : base(Guid.NewGuid())
        {
            TenantId = tenantId;
            PhoneNumberId = phoneNumberId;
            EncryptedAccessToken = encryptedAccessToken;
            IsActive = true;
        }

        public static Result<TenantWhatsAppSettings> Create(Guid tenantId, string phoneNumberId, string encryptedAccessToken)
        {
            if (string.IsNullOrWhiteSpace(phoneNumberId))
                return Result.Failure<TenantWhatsAppSettings>(Error.Validation("WhatsAppSettings.PhoneIdRequired", "Phone Number ID is required."));

            if (string.IsNullOrWhiteSpace(encryptedAccessToken))
                return Result.Failure<TenantWhatsAppSettings>(Error.Validation("WhatsAppSettings.TokenRequired", "Access Token is required."));

            return new TenantWhatsAppSettings(tenantId, phoneNumberId, encryptedAccessToken);
        }

        public Result UpdateCredentials(string phoneNumberId, string encryptedAccessToken)
        {
            if (string.IsNullOrWhiteSpace(phoneNumberId))
                return Result.Failure(Error.Validation("WhatsAppSettings.PhoneIdRequired", "Phone Number ID is required."));

            if (string.IsNullOrWhiteSpace(encryptedAccessToken))
                return Result.Failure(Error.Validation("WhatsAppSettings.TokenRequired", "Access Token is required."));

            PhoneNumberId = phoneNumberId;
            EncryptedAccessToken = encryptedAccessToken;

            return Result.Success();
        }

        public void Activate() => IsActive = true;

        public void Deactivate() => IsActive = false;
    }
}
