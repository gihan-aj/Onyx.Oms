using FluentValidation;

namespace Onyx.Oms.Web.Features.Customers.GetCustomerOrderHistory
{
    public class GetCustomerOrderHistoryQueryValidator : AbstractValidator<GetCustomerOrderHistoryQuery>
    {
        public GetCustomerOrderHistoryQueryValidator()
        {
            RuleFor(x => x.CustomerId)
                .NotEmpty()
                .WithMessage("Customer ID is required.");
            RuleFor(x => x.Top)
                .GreaterThan(0)
                .LessThanOrEqualTo(100)
                .WithMessage("Top must be between 1 and 100.");
        }
    }
}
