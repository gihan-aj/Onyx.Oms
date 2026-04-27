using FluentValidation;

namespace Onyx.Oms.Web.Features.Orders.UpdateOrderLogistics
{
    public class UpdateOrderLogisticsValidator : AbstractValidator<UpdateOrderLogisticsCommand>
    {
        public UpdateOrderLogisticsValidator()
        {
            RuleFor(x => x.OrderId).NotEmpty();

            RuleFor(x => x.ShippingAddress!)
                .SetValidator(new UpdateShippingAddressDtoValidator())
                .When(x => x.ShippingAddress != null);
        }
    }

    public class UpdateShippingAddressDtoValidator : AbstractValidator<UpdateShippingAddressDto>
    {
        public UpdateShippingAddressDtoValidator()
        {
            RuleFor(x => x.Street).MaximumLength(200);
            RuleFor(x => x.City).MaximumLength(100);
            RuleFor(x => x.District).MaximumLength(100);
            RuleFor(x => x.State).MaximumLength(100);
            RuleFor(x => x.PostalCode).MaximumLength(20);
            RuleFor(x => x.Country).MaximumLength(100);
        }
    }
}
