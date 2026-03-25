using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Onyx.Oms.Core.Domain.Entities;

namespace Onyx.Oms.Infrastructure.Persistence.Configurations;

public class SubscriptionPlanConfiguration : IEntityTypeConfiguration<SubscriptionPlan>
{
    public void Configure(EntityTypeBuilder<SubscriptionPlan> builder)
    {
        builder.ToTable("SubscriptionPlans");

        builder.HasKey(sp => sp.Id);

        builder.Property(sp => sp.Name).IsRequired().HasMaxLength(100);

        builder.ComplexProperty(sp => sp.MonthlyPrice, p =>
        {
            p.Property(m => m.Amount).HasColumnName("PriceAmount").HasPrecision(18,2);
            p.Property(m => m.Currency).HasColumnName("PriceCurrency").HasMaxLength(3);
        });

        builder.Property(sp => sp.MaxUsersAllowed).IsRequired();
        builder.Property(sp => sp.MaxOrdersAllowed).IsRequired();
        builder.Property(sp => sp.TrialPeriodInDays).IsRequired();
        builder.Property(sp => sp.IsActive).HasDefaultValue(true);

        builder.Property(c => c.CreatedBy).HasMaxLength(36);
        builder.Property(c => c.LastModifiedBy).HasMaxLength(36);
    }
}
