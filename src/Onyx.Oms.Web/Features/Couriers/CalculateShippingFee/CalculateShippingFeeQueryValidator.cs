using FluentValidation;

namespace Onyx.Oms.Web.Features.Couriers.CalculateShippingFee
{
    public class CalculateShippingFeeQueryValidator : AbstractValidator<CalculateShippingFeeQuery>
    {
        public CalculateShippingFeeQueryValidator()
        {
            RuleFor(x => x.CourierId).NotEmpty().WithMessage("CourierId is required.");
            RuleFor(x => x.District).NotEmpty().WithMessage("District is required.");
            RuleFor(x => x.TotalWeightKg).GreaterThan(0).WithMessage("TotalWeightKg must be greater than 0.");
            RuleFor(x => x.CodAmount).GreaterThanOrEqualTo(0).WithMessage("CodAmount cannot be negative.");
        }
    }
}