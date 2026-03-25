
namespace Onyx.Oms.Core.Common.Interfaces;

public interface ICurrentUserService
{
    string? UserId { get; }
    string? UserName { get; }
    string? Email { get; }
    string? ClientId { get; }
    string? TenantId { get; }
    bool IsAuthenticated { get; }
}
