using FluentValidation;

namespace Onyx.Oms.Web.Features.Settings.AppSequences.UpdateAppSequenceValue;

public class UpdateAppSequenceValueValidator : AbstractValidator<UpdateAppSequenceValueCommand>
{
    public UpdateAppSequenceValueValidator()
    {
        RuleFor(x => x.SequenceId)
            .NotEmpty().WithMessage("Sequence ID is required.")
            .MaximumLength(10).WithMessage("Sequence ID cannot exceed 10 characters.");

        RuleFor(x => x.NewValue)
            .GreaterThanOrEqualTo(0).WithMessage("New value must be greater than or equal to 0.");
    }
}
