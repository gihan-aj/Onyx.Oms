using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Enums;
using Onyx.Oms.Core.Domain.Models;

namespace Onyx.Oms.Core.Domain.Entities
{
    public class PaymentMethodConfig : AuditableEntity<Guid>, IMustHaveTenant
    {
        public Guid TenantId { get; private set; }
        public PaymentMethod Type { get; private set; }
        public string DisplayName { get; private set; } = string.Empty;
        public decimal FeeRate { get; private set; } = 0;
        public bool IsActive { get; private set; }

        private PaymentMethodConfig() { }

        private PaymentMethodConfig(Guid tenantId, PaymentMethod type, string name, decimal feeRate = 0) : base(Guid.NewGuid())
        {
            TenantId = tenantId;
            Type = type;
            DisplayName = name;
            FeeRate = feeRate;
            IsActive = true;
        }

        public static Result<PaymentMethodConfig> Create(Guid tenantId, PaymentMethod type, string name, decimal feeRate = 0)
        {
            if (string.IsNullOrWhiteSpace(name))
                return Result.Failure<PaymentMethodConfig>(Error.Validation("PaymentMethodConfig.DisplayNameRequired", "Display name cannot be empty."));

            if (feeRate < 0)
                return Result.Failure<PaymentMethodConfig>(Error.Validation("PaymentMethodConfig.InvalidFeeRate", "Fee rate cannot be negative."));

            var config = new PaymentMethodConfig(tenantId, type, name, feeRate);
            return Result.Success(config);
        }

        public Result Update(string name, decimal feeRate)
        {
            if (string.IsNullOrWhiteSpace(name))
                return Result.Failure(Error.Validation("PaymentMethodConfig.DisplayNameRequired", "Display name cannot be empty."));

            if(feeRate < 0)
                return Result.Failure(Error.Validation("PaymentMethodConfig.InvalidFeeRate", "Fee rate cannot be negative."));

            DisplayName = name;
            FeeRate = feeRate;

            return Result.Success();
        }

        public void Activate() => IsActive = true;
        public void Deactivate() => IsActive = false;
    }
}
