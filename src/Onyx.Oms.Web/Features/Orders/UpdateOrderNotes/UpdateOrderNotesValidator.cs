using FluentValidation;

namespace Onyx.Oms.Web.Features.Orders.UpdateOrderNotes
{
    public class UpdateOrderNotesValidator : AbstractValidator<UpdateOrderNotesCommand>
    {
        public UpdateOrderNotesValidator()
        {
            RuleFor(x => x.OrderId).NotEmpty();
            RuleFor(x => x.Notes).MaximumLength(2000);
        }
    }
}
