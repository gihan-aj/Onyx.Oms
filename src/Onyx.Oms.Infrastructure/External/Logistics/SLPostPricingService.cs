using Onyx.Oms.Core.Common.Interfaces;

namespace Onyx.Oms.Infrastructure.External.Logistics
{
    internal class SLPostPricingService : ISLPostPricingService
    {
        public decimal CalculateFee(decimal weightKg)
        {
            decimal weightGrams = weightKg * 1000;

            if (weightGrams <= 250) return 200m;
            if (weightGrams <= 500) return 250m;
            if (weightGrams <= 1000) return 350m;
            if (weightGrams <= 2000) return 400m;
            if (weightGrams <= 3000) return 450m;
            if (weightGrams <= 4000) return 500m;
            if (weightGrams <= 5000) return 550m;
            if (weightGrams <= 6000) return 600m;
            if (weightGrams <= 7000) return 650m;
            if (weightGrams <= 8000) return 700m;
            if (weightGrams <= 9000) return 750m;
            if (weightGrams <= 10000) return 800m;
            if (weightGrams <= 15000) return 850m;
            if (weightGrams <= 20000) return 1100m;
            if (weightGrams <= 25000) return 1600m;
            if (weightGrams <= 30000) return 2100m;
            if (weightGrams <= 35000) return 2600m;
            if (weightGrams <= 40000) return 3100m;

            // don't deliver
            return 0m;
        }
    }
}
