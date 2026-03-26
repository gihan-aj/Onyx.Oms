using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Users.RegisterUser
{
    public record RegisterUserCommand(
        UserDetailsDto UserDetails,
        CompanyDetailsDto CompanyDetails,
        SubscriptionDetailsDto SubscriptionDetails) : ICommand<Guid>;

    public record UserDetailsDto(
        string FirstName,
        string LastName,
        string Email,
        string Password,
        string ConfirmPassword);

    public record CompanyDetailsDto(
        string CompanyName,
        string ContactEmail);

    public record SubscriptionDetailsDto(Guid SubscriptionId);
}
