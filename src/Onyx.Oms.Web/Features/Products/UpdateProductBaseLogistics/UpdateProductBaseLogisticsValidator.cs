using FluentValidation;

namespace Onyx.Oms.Web.Features.Products.UpdateProductBaseLogistics
{
    public class UpdateProductBaseLogisticsValidator : AbstractValidator<UpdateProductBaseLogisticsCommand>
    {
        public UpdateProductBaseLogisticsValidator()
        {
            RuleFor(x => x.BasePrice)
                .NotNull().WithMessage("Base Price is required.")
                .Must(x => x != null && x.Amount >= 0).WithMessage("Base Price cannot be negative.");

            RuleFor(x => x.BaseCost)
                .NotNull().WithMessage("Base Cost is required.")
                .Must(x => x != null && x.Amount >= 0).WithMessage("Base Cost cannot be negative.");

            When(x => x.BaseWeight != null, () =>
            {
                RuleFor(x => x.BaseWeight!.Value)
                    .GreaterThanOrEqualTo(0).WithMessage("Weight cannot be negative.");
            });
        }
    }
}
