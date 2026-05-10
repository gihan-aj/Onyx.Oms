using FluentValidation;

namespace Onyx.Oms.Web.Features.Orders.GetOrderStatusCounts
{
    public class GetOrderStatusCountsValidator : AbstractValidator<GetOrderStatusCountsQuery>
    {
        public GetOrderStatusCountsValidator()
        {
            // Date validation rules can be added here
            RuleFor(x => x.ToDate)
                .GreaterThanOrEqualTo(x => x.FromDate)
                .When(x => x.FromDate.HasValue && x.ToDate.HasValue)
                .WithMessage("ToDate must be greater than or equal to FromDate.");
        }
    }
}
