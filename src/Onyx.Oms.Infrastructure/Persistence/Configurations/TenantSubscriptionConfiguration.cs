using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Onyx.Oms.Core.Domain.Entities;

namespace Onyx.Oms.Infrastructure.Persistence.Configurations;

public class TenantSubscriptionConfiguration : IEntityTypeConfiguration<TenantSubscription>
{
    public void Configure(EntityTypeBuilder<TenantSubscription> builder)
    {
        builder.ToTable("TenantSubscriptions");

        builder.HasKey(ts => ts.Id);
        
        builder.Property(ts => ts.SubscriptionId).IsRequired();
        
        builder.Property(ts => ts.Status).IsRequired().HasConversion<int>();

        builder.Property(c => c.CreatedBy).HasMaxLength(36);
        builder.Property(c => c.LastModifiedBy).HasMaxLength(36);

        builder.HasOne(ts => ts.Plan)
            .WithMany()
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
    }
}
