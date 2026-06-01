using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Entities;
using Onyx.Oms.Infrastructure.Persistence.Entities;
using Onyx.Oms.Infrastructure.Persistence.Interceptors;

namespace Onyx.Oms.Infrastructure.Persistence;

public class AppDbContext : DbContext, IApplicationDbContext
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ITenantSecurityBypass _bypass;

    public AppDbContext(
        DbContextOptions<AppDbContext> options,
        ICurrentUserService currentUserService,
        ITenantSecurityBypass bypass) : base(options)
    {
        _currentUserService = currentUserService;
        _bypass = bypass;
    }

    public DbSet<Courier> Couriers { get; set; }
    public DbSet<CourierZoneRate> CourierZoneRates { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<FulfillmentTask> FulfillmentTasks => Set<FulfillmentTask>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<OrderPayment> OrderPayments => Set<OrderPayment>();
    public DbSet<PaymentMethodConfig> PaymentMethodConfigs => Set<PaymentMethodConfig>();
    public DbSet<AppUser> AppUsers => Set<AppUser>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<AppSequence> AppSequences => Set<AppSequence>();
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<TenantWhatsAppSettings> TenantWhatsAppSettings => Set<TenantWhatsAppSettings>();
    public DbSet<TenantSubscription> TenantSubscriptions => Set<TenantSubscription>();
    public DbSet<SubscriptionPlan> SubscriptionPlans => Set<SubscriptionPlan>();
    public DbSet<Expense> Expenses => Set<Expense>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // STATIC schmea rules
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // DYNAMIC security filters
        var activeTenantId = _currentUserService.ActiveTenantId;

        foreach(var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(IMustHaveTenant).IsAssignableFrom(entityType.ClrType))
            {
                var method = typeof(AppDbContext)
                    .GetMethod(nameof(ApplyTenantFilter), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.MakeGenericMethod(entityType.ClrType);

                method?.Invoke(this, new object[] { modelBuilder });
            }
        }
    }

    private void ApplyTenantFilter<TEntity>(ModelBuilder builder)
        where TEntity : class, IMustHaveTenant
    {
        if (typeof(ISoftDeletable).IsAssignableFrom(typeof(TEntity)))
        {
            builder.Entity<TEntity>().HasQueryFilter(e => 
                (_bypass.IsBypassEnabled || e.TenantId == _currentUserService.ActiveTenantId)
                && EF.Property<DateTimeOffset?>(e, "DeletedAtUtc") == null);
        }
        else
        {
            builder.Entity<TEntity>().HasQueryFilter(e => 
                _bypass.IsBypassEnabled ||
                e.TenantId == _currentUserService.ActiveTenantId);
        }
    }
}
