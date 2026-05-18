using Onyx.Oms.Core.Domain.Entities;
using Onyx.Oms.Core.Domain.Enums;

namespace Onyx.Oms.Core.Domain.Constants
{
    public static class DefaultPaymentMethods
    {
        public static IEnumerable<PaymentMethodConfig> GetConfigs(Guid tenantId)
        {
            var configs = new List<PaymentMethodConfig>
            {
                PaymentMethodConfig.Create(tenantId, PaymentMethod.CashOnDelivery, "Cash on Delivery", 0m).Value,
                PaymentMethodConfig.Create(tenantId, PaymentMethod.BankTransfer, "Bank Transfer", 0m).Value,
                PaymentMethodConfig.Create(tenantId, PaymentMethod.Cash, "Cash", 0m).Value,
                PaymentMethodConfig.Create(tenantId, PaymentMethod.PayHere, "PayHere", 0.03m).Value,
                PaymentMethodConfig.Create(tenantId, PaymentMethod.Koko, "Koko", 0.05m).Value
            };

            return configs;
        }
    }
}
