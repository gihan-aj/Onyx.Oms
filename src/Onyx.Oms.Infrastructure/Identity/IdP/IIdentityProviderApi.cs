using Refit;

namespace Onyx.Oms.Infrastructure.Identity.IdP;

public interface IIdentityProviderApi
{
    [Post("/api/roles")]
    Task<IApiResponse> CreateRoleAsync([Body] CreateRoleRequest request);

    [Post("/api/users")]
    Task<IApiResponse<UserDto>> InviteUserAsync([Body] InviteUserRequest request);
    
    [Post("/api/users/{userId}/roles")]
    Task<IApiResponse> AssignRoleAsync(Guid userId, [Body] AssignRoleRequest request);
}

public record CreateRoleRequest(string Name);
public record InviteUserRequest(string Email, string RoleName, string FirstName, string LastName);
public record AssignRoleRequest(string RoleName);

public record UserDto(Guid Id, string Email, string FirstName, string LastName, bool IsActive);
