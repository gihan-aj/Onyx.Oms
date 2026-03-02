using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Domain.Entities;
using Onyx.Oms.Infrastructure.Persistence.Entities;
using Onyx.Oms.Infrastructure.Persistence.Interceptors;

namespace Onyx.Oms.Infrastructure.Persistence;

public class AppDbContext : DbContext, IApplicationDbContext
{
    private readonly AuditableEntityInterceptor _auditableEntityInterceptor;

    public AppDbContext(
        DbContextOptions<AppDbContext> options,
        AuditableEntityInterceptor auditableEntityInterceptor) : base(options)
    {
        _auditableEntityInterceptor = auditableEntityInterceptor;
    }

    public DbSet<Courier> Couriers { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();
    //public DbSet<Product> Products => Set<Product>();
    //public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    //public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<AppUser> AppUsers => Set<AppUser>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<AppSequence> AppSequences => Set<AppSequence>();
    public DbSet<TenantProfile> TenantProfiles => Set<TenantProfile>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(_auditableEntityInterceptor);
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
