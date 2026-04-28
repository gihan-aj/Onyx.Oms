using FluentValidation;

namespace Onyx.Oms.Web.Features.Orders.GetOrderById
{
    public class GetOrderByIdValidator : AbstractValidator<GetOrderByIdQuery>
    {
        public GetOrderByIdValidator()
        {
            RuleFor(x => x.OrderId).NotEmpty();
        }
    }
}
