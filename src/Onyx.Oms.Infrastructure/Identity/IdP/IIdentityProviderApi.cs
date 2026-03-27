using Refit;

namespace Onyx.Oms.Infrastructure.Identity.IdP;

public interface IIdentityProviderApi
{
    //[Post("/api/roles")]
    //Task<IApiResponse> CreateRoleAsync([Body] CreateRoleRequest request);
    
    //[Put("/api/roles/{name}/name")]
    //Task<IApiResponse> UpdateRoleNameAsync(string name, [Body] UpdateRoleNameRequest request);

    //[Delete("/api/roles/{name}")]
    //Task<IApiResponse> DeleteRoleAsync(string name, [AliasAs("targetClientId")] string targetClientId);

    //[Get("/api/users")]
    //Task<IApiResponse<UserDto>> GetUserByEmailAsync([Query] string email);

    [Post("/api/users")]
    Task<IApiResponse<UserDto>> InviteUserAsync([Body] InviteUserRequest request);

    [Post("/api/users/register")]
    Task<IApiResponse<RegisterUserResponse>> RegisterUserAsync([Body] RegisterUserRequest request);
    
    //[Post("/api/users/{userId}/roles")]
    //Task<IApiResponse<AssignRolesResponse>> AssignRolesAsync(Guid userId, [Body] AssignRolesRequest request);
}

//public record CreateRoleRequest(string Name, string TargetClientId);
//public record UpdateRoleNameRequest(string NewName, string TargetClientId);
public record InviteUserRequest(string Email, IEnumerable<string> RoleNames, string FirstName, string LastName, string TargetClientId);
//public record AssignRolesRequest(IEnumerable<string> RoleNames, string TargetClientId);

public record UserDto(Guid Id, string Email, string FirstName, string LastName, bool IsActive, IEnumerable<string>? AssignedRoles = null);
//public record AssignRolesResponse(string Message, IEnumerable<string> AssignedRoles);

public record RegisterUserRequest(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string TenantId);
public record RegisterUserResponse(Guid UserId);
