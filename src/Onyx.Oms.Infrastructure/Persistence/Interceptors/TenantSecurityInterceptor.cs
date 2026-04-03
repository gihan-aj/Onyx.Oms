using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Onyx.Oms.Core.Common.Interfaces;

namespace Onyx.Oms.Infrastructure.Persistence.Interceptors;

public class TenantSecurityInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ITenantSecurityBypass _bypass;

    public TenantSecurityInterceptor(ICurrentUserService currentUserService, ITenantSecurityBypass bypass)
    {
        _currentUserService = currentUserService;
        _bypass = bypass;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        EnforceTenantSecurity(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        EnforceTenantSecurity(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void EnforceTenantSecurity(DbContext? context)
    {
        if (context == null) return;

        if (_bypass.IsBypassEnabled)
            return;

        var activeTenantId = _currentUserService.ActiveTenantId;

        // Only need to check entities that are bound to a tenant
        foreach (var entry in context.ChangeTracker.Entries<IMustHaveTenant>())
        {
            // Check both Added and Modified to prevent someone from maliciously
            // changing the TenantId of an existing record to move it to another business.
            if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
            {
                // SAFETY CHECK 1: Did they forget the TenantId entirely?
                if (entry.Entity.TenantId == Guid.Empty)
                {
                    throw new InvalidOperationException($"CRITICAL: Attempted to save {entry.Entity.GetType().Name} without a TenantId!");
                }

                // SAFETY CHECK 2: Is the entity trying to save to a DIFFERENT tenant?
                if (entry.Entity.TenantId != activeTenantId)
                {
                    throw new UnauthorizedAccessException($"CRITICAL: Cross-tenant data injection attempt detected on {entry.Entity.GetType().Name}. Expected: {activeTenantId}, Actual: {entry.Entity.TenantId}");
                }
            }
        }
    }
}
