using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Onyx.Oms.Core.Domain.Entities;

namespace Onyx.Oms.Infrastructure.Persistence.Configurations;

public class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
{
    public void Configure(EntityTypeBuilder<ProductVariant> builder)
    {
        builder.HasKey(v => v.Id);

        builder.Property(v => v.Sku)
            .HasMaxLength(50)
            .IsRequired();

        // Unique SKU
        builder.HasIndex(v => v.Sku)
            .IsUnique();

        builder.Property(v => v.Name)
            .HasMaxLength(200);

        builder.Property(v => v.Size)
            .HasMaxLength(50);

        builder.Property(v => v.Color)
            .HasMaxLength(50);

        builder.Property(v => v.Price)
            .HasPrecision(18, 2);

        builder.Property(v => v.Cost)
            .HasPrecision(18, 2);

        builder.Property(v => v.Weight)
            .HasPrecision(18, 3);
    }
}
