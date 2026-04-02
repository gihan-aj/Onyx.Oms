using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Domain.Entities;

namespace Onyx.Oms.Core.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Courier> Couriers { get; }
    DbSet<Customer> Customers { get; }
    DbSet<ProductCategory> ProductCategories { get; }
    DbSet<Product> Products { get; }
    DbSet<ProductVariant> ProductVariants { get; }
    DbSet<ProductImage> ProductImages { get; }
    DbSet<AppUser> AppUsers { get; }
    DbSet<Role> Roles { get; }
    //DbSet<TenantProfile> TenantProfiles { get; }
    DbSet<Tenant> Tenants { get; }
    DbSet<TenantSubscription> TenantSubscriptions { get; }
    DbSet<SubscriptionPlan> SubscriptionPlans { get; }
    Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker ChangeTracker { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
