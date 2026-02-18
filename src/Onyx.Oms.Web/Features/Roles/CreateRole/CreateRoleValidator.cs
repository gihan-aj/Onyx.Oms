using FluentValidation;

namespace Onyx.Oms.Web.Features.Roles.CreateRole;

public class CreateRoleValidator : AbstractValidator<CreateRoleCommand>
{
    public CreateRoleValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Role name is required.")
            .MaximumLength(50).WithMessage("Role name must not exceed 50 characters.")
            .Matches("^[a-zA-Z0-9_]*$").WithMessage("Role name can only contain letters, numbers, and underscores.");

        RuleFor(x => x.Description)
            .MaximumLength(200);
            
         RuleFor(x => x.Permissions)
            .NotNull();
    }
}
