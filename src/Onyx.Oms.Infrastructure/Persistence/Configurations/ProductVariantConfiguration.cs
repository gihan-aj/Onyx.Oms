using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Onyx.Oms.Core.Domain.Entities;

namespace Onyx.Oms.Infrastructure.Persistence.Configurations;

public class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
{
    public void Configure(EntityTypeBuilder<ProductVariant> builder)
    {
        builder.ToTable("ProductVariants");
        builder.HasKey(v => v.Id);
        builder.HasIndex(v => v.Sku).IsUnique();

        builder.Property(v => v.Sku).IsRequired().HasMaxLength(100);

        builder.Ignore(v => v.DisplayName);
        builder.Ignore(v => v.AvailableQuantity);

        // Allow Nulls for Optional Attributes
        builder.Property(v => v.Color).IsRequired(false).HasMaxLength(50);
        builder.Property(v => v.Size).IsRequired(false).HasMaxLength(50);

        builder.ComplexProperty(v => v.Price, pb => {
            pb.Property(m => m.Amount).HasColumnName("PriceAmount").HasPrecision(18, 2);
            pb.Property(m => m.Currency).HasColumnName("PriceCurrency").HasMaxLength(3);
        });

        builder.ComplexProperty(v => v.Cost, cb => {
            cb.Property(m => m.Amount).HasColumnName("CostAmount").HasPrecision(18, 2);
            cb.Property(m => m.Currency).HasColumnName("CostCurrency").HasMaxLength(3);
        });

        builder.ComplexProperty(v => v.Weight, wb => {
            wb.Property(w => w.Value).HasColumnName("WeightValue").HasPrecision(10, 3);
            wb.Property(w => w.Unit).HasColumnName("WeightUnit").HasMaxLength(10);
        });

        builder.Property(c => c.CreatedBy)
            .HasMaxLength(36);

        builder.Property(c => c.LastModifiedBy)
             .HasMaxLength(36);
    }
}
