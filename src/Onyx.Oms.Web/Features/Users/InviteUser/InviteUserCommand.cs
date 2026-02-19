using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Users.InviteUser;

public record InviteUserCommand(
    string Email,
    string FirstName,
    string LastName,
    Guid RoleId) : ICommand<Guid>;
