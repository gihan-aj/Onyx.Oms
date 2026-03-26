using FluentValidation;

namespace Onyx.Oms.Web.Features.Users.RegisterUser
{
    public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
    {
        public RegisterUserCommandValidator()
        {
            RuleFor(x => x.UserDetails).NotNull().WithMessage("User details are required.");
            RuleFor(x => x.CompanyDetails).NotNull().WithMessage("Company details are required.");
            RuleFor(x => x.SubscriptionDetails).NotNull().WithMessage("Subscription details are required.");
            When(x => x.UserDetails != null, () =>
            {
                RuleFor(x => x.UserDetails.FirstName).NotEmpty().WithMessage("First name is required.");
                RuleFor(x => x.UserDetails.LastName).NotEmpty().WithMessage("Last name is required.");
                RuleFor(x => x.UserDetails.Email).NotEmpty().EmailAddress().WithMessage("A valid email is required.");
                RuleFor(x => x.UserDetails.Password).NotEmpty().MinimumLength(6).WithMessage("Password must be at least 6 characters long.");
                RuleFor(x => x.UserDetails.ConfirmPassword)
                    .Equal(x => x.UserDetails.Password)
                    .WithMessage("Passwords do not match.");
            });
            When(x => x.CompanyDetails != null, () =>
            {
                RuleFor(x => x.CompanyDetails.CompanyName).NotEmpty().WithMessage("Company name is required.");
                RuleFor(x => x.CompanyDetails.ContactEmail).NotEmpty().EmailAddress().WithMessage("A valid contact email is required.");
            });
            When(x => x.SubscriptionDetails != null, () =>
            {
                RuleFor(x => x.SubscriptionDetails.SubscriptionId).NotEmpty().WithMessage("Subscription ID is required.");
            });
        }
    }
}
