
namespace Onyx.Oms.Core.Common.Interfaces;

public interface ICurrentUserService
{
    bool IsAuthenticated { get; }

    Guid? UserId { get; }
    Guid? ActualTenantId { get; } // The tenant they ACTUALLY belong to (from their JWT)

    Guid? ActiveTenantId { get; } // The tenant their queries should run against (handles impersonation)
    bool IsImpersonating { get; }

    Task<Guid?> GetActiveTenantIdAsync(CancellationToken cancellationToken = default);
}
