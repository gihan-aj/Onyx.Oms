using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Domain.ValueObjects;

namespace Onyx.Oms.Core.Domain.Entities
{
    public class CourierZoneRate : Entity<Guid>, IMustHaveTenant
    {
        public Guid TenantId { get; private set; }
        public Guid CourierId { get; private set; }
        public virtual Courier Courier { get; private set; } = null!;
        public string ZoneName { get; private set; } = string.Empty;
        public Money BaseFee { get; private set; } = Money.Zero();
        public Weight BaseWeight { get; private set; } = Weight.Zero();
        public Money ExcessFeePerWeightUnit { get; private set; } = Money.Zero();
        public decimal CodPercentage { get; private set; }
        public bool IsDefault { get; private set; }

        private readonly List<string> _coveredDistrics = new();
        public IReadOnlyCollection<string> CoveredDistrics => _coveredDistrics.AsReadOnly();

        private CourierZoneRate() { }

        private CourierZoneRate(
            Guid tenantId,
            Guid courierId,
            string zoneName,
            Money baseFee,
            Weight baseWeight,
            Money excessFeePerWeightUnit,
            decimal codPercentage,
            bool isDefault,
            List<string> coveredDistricts) : base(Guid.NewGuid())
        {
            TenantId = tenantId;
            CourierId = courierId;
            ZoneName = zoneName;
            BaseFee = baseFee;
            BaseWeight = baseWeight;
            ExcessFeePerWeightUnit = excessFeePerWeightUnit;
            CodPercentage = codPercentage;
            IsDefault = isDefault;
            if (coveredDistricts != null && coveredDistricts.Any())
                _coveredDistrics.AddRange(coveredDistricts);
        }

        internal static Result<CourierZoneRate> Create(
            Guid tenantId,
            Guid courierId,
            string zoneName,
            decimal baseFee,
            decimal baseWeight,
            decimal excessFeePerWeightUnit,
            decimal codePercentage,
            string currency,
            string weightUnit,
            bool isDefault,
            List<string> coveredDistricts)
        {
            if (tenantId == Guid.Empty)
                return Result.Failure<CourierZoneRate>(Error.Validation("CourierZoneRate.TenantIdRequired", "Tenant ID is required."));

            if (courierId == Guid.Empty)
                return Result.Failure<CourierZoneRate>(Error.Validation("CourierZoneRate.CourierIdRequired", "Courier ID is required."));

            if (string.IsNullOrEmpty(zoneName))
                return Result.Failure<CourierZoneRate>(Error.Validation("CourierZoneRate.ZoneNameRequired", "Zone Name is required."));

            if (baseFee < 0)
                return Result.Failure<CourierZoneRate>(Error.Validation("CourierZoneRate.BaseFeeInvalid", "Base Fee cannot be negative."));

            if (baseWeight < 0)
                return Result.Failure<CourierZoneRate>(Error.Validation("CourierZoneRate.BaseWeightInvalid", "Base Weight cannot be negative."));

            if (excessFeePerWeightUnit < 0)
                return Result.Failure<CourierZoneRate>(Error.Validation("CourierZoneRate.ExcessFeePerWeightUnitInvalid", "Excess Fee Per Weight Unit cannot be negative."));

            if (codePercentage < 0 || codePercentage > 100)
                return Result.Failure<CourierZoneRate>(Error.Validation("CourierZoneRate.CodPercentageInvalid", "COD Percentage cannot be negative or more than 100."));

            var baseFeeMoney = new Money(baseFee, currency);
            var baseWeightObj = new Weight(baseWeight, weightUnit);
            var excessFeeMoney = new Money(excessFeePerWeightUnit, currency);

            var courierZoneRate = new CourierZoneRate(
                tenantId,
                courierId,
                zoneName,
                baseFeeMoney,
                baseWeightObj,
                excessFeeMoney,
                codePercentage,
                isDefault,
                coveredDistricts);

            return courierZoneRate;
        }

        internal Result Update(
            string zoneName,
            decimal baseFee,
            decimal baseWeight,
            decimal excessFeePerWeightUnit,
            decimal codePercentage,
            string currency,
            string weightUnit,
            bool isDefault,
            List<string> coveredDistricts)
        {
            if (string.IsNullOrEmpty(zoneName))
                return Result.Failure(Error.Validation("CourierZoneRate.ZoneNameRequired", "Zone Name is required."));
            if (baseFee < 0)
                return Result.Failure(Error.Validation("CourierZoneRate.BaseFeeInvalid", "Base Fee cannot be negative."));
            if (baseWeight < 0)
                return Result.Failure(Error.Validation("CourierZoneRate.BaseWeightInvalid", "Base Weight cannot be negative."));
            if (excessFeePerWeightUnit < 0)
                return Result.Failure(Error.Validation("CourierZoneRate.ExcessFeePerWeightUnitInvalid", "Excess Fee Per Weight Unit cannot be negative."));
            if (codePercentage < 0 || codePercentage > 100)
                return Result.Failure(Error.Validation("CourierZoneRate.CodPercentageInvalid", "COD Percentage cannot be negative or more than 100."));
            ZoneName = zoneName;
            BaseFee = new Money(baseFee, currency);
            BaseWeight = new Weight(baseWeight, weightUnit);
            ExcessFeePerWeightUnit = new Money(excessFeePerWeightUnit, currency);
            CodPercentage = codePercentage;
            IsDefault = isDefault;
            _coveredDistrics.Clear();
            if (coveredDistricts != null && coveredDistricts.Any())
                _coveredDistrics.AddRange(coveredDistricts);
            return Result.Success();
        }

        public decimal CalculateShippingFee(decimal totalOrderWeightKg, decimal totalCodAmount)
        {
            decimal shippingFee = BaseFee.Amount;

            if(totalOrderWeightKg > BaseWeight.Value)
            {
                decimal excessWeight = Math.Ceiling(totalOrderWeightKg -  BaseWeight.Value);
                shippingFee += (excessWeight * ExcessFeePerWeightUnit.Amount);
            }

            if(totalCodAmount > 0 && CodPercentage > 0)
            {
                shippingFee += (totalCodAmount * (CodPercentage / 100m));
            }

            return shippingFee;
        }
    }
}
