
namespace Onyx.Oms.Core.Common.Interfaces;

public interface ICurrentUserService
{
    Guid UserId { get; }
    Guid ActualTenantId { get; } // The tenant they ACTUALLY belong to (from their JWT)
    Guid ActiveTenantId { get; } // The tenant their queries should run against (handles impersonation)

    bool IsAuthenticated { get; }
    bool IsImpersonating { get; }
}
