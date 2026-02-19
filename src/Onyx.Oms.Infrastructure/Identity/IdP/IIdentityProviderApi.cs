using Refit;

namespace Onyx.Oms.Infrastructure.Identity.IdP;

public interface IIdentityProviderApi
{
    [Post("/api/roles")]
    Task<IApiResponse> CreateRoleAsync([Body] CreateRoleRequest request);
    
    [Put("/api/roles/{name}/name")]
    Task<IApiResponse> UpdateRoleNameAsync(string name, [Body] UpdateRoleNameRequest request);

    [Delete("/api/roles/{name}")]
    Task<IApiResponse> DeleteRoleAsync(string name, [AliasAs("targetClientId")] string targetClientId);

    [Post("/api/users")]
    Task<IApiResponse<UserDto>> InviteUserAsync([Body] InviteUserRequest request);
    
    [Post("/api/users/{userId}/roles")]
    Task<IApiResponse> AssignRoleAsync(Guid userId, [Body] AssignRoleRequest request);
}

public record CreateRoleRequest(string Name, string TargetClientId);
public record UpdateRoleNameRequest(string NewName, string TargetClientId);
public record InviteUserRequest(string Email, string RoleName, string FirstName, string LastName, string TargetClientId);
public record AssignRoleRequest(string RoleName, string TargetClientId);

public record UserDto(Guid Id, string Email, string FirstName, string LastName, bool IsActive);
