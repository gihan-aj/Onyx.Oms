using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Domain.Entities;

namespace Onyx.Oms.Core.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Courier> Couriers { get; }
    DbSet<CourierZoneRate> CourierZoneRates { get; set; }
    DbSet<Customer> Customers { get; }
    DbSet<ProductCategory> ProductCategories { get; }
    DbSet<Product> Products { get; }
    DbSet<ProductVariant> ProductVariants { get; }
    DbSet<ProductImage> ProductImages { get; }
    DbSet<FulfillmentTask> FulfillmentTasks {  get; }
    DbSet<Order> Orders { get; }
    DbSet<OrderItem> OrderItems { get; }
    DbSet<OrderPayment> OrderPayments { get; }
    DbSet<PaymentMethodConfig> PaymentMethodConfigs {  get; }
    DbSet<AppUser> AppUsers { get; }
    DbSet<Role> Roles { get; }
    //DbSet<TenantProfile> TenantProfiles { get; }
    DbSet<Tenant> Tenants { get; }
    DbSet<TenantWhatsAppSettings> TenantWhatsAppSettings { get; }
    DbSet<TenantSubscription> TenantSubscriptions { get; }
    DbSet<SubscriptionPlan> SubscriptionPlans { get; }
    DbSet<Expense> Expenses { get; }
    Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker ChangeTracker { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
