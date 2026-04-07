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

        builder.Property(v => v.TenantId)
            .IsRequired();

        builder.HasIndex(v => v.Sku)
            .IsUnique()
            .HasFilter("[DeletedAtUtc] IS NULL");

        builder.Property(v => v.Sku)
            .IsRequired()
            .HasMaxLength(100);

        // Computed / transient properties — not persisted
        builder.Ignore(v => v.DisplayName);
        builder.Ignore(v => v.AvailableQuantity);
        // IsDeleted is computed from DeletedAtUtc — ignore the property and filter on DeletedAtUtc directly
        builder.Ignore(v => v.IsDeleted);

        // Money — ComplexProperty (non-nullable inline columns)
        builder.ComplexProperty(v => v.Price, pb =>
        {
            pb.Property(m => m.Amount).HasColumnName("PriceAmount").HasPrecision(18, 2);
            pb.Property(m => m.Currency).HasColumnName("PriceCurrency").HasMaxLength(3);
        });

        builder.ComplexProperty(v => v.Cost, pb =>
        {
            pb.Property(m => m.Amount).HasColumnName("CostAmount").HasPrecision(18, 2);
            pb.Property(m => m.Currency).HasColumnName("CostCurrency").HasMaxLength(3);
        });

        // Weight — nullable; use OwnsOne (nullable columns in DB)
        builder.OwnsOne(v => v.Weight, wb =>
        {
            wb.Property(w => w.Value).HasColumnName("WeightValue").HasPrecision(10, 3);
            wb.Property(w => w.Unit).HasColumnName("WeightUnit").HasMaxLength(10);
        });

        // Attributes — List<VariantAttribute> stored as JSON column (EF8+ ToJson)
        // Empty list for default (variant-less) variants is stored as "[]"
        builder.OwnsMany(v => v.Attributes, ab =>
        {
            ab.ToJson();
            ab.Property(a => a.Name).HasMaxLength(100);
            ab.Property(a => a.Value).HasMaxLength(200);
        });

        builder.HasIndex(v => new { v.TenantId, v.ProductId });

        // Soft-delete global query filter — uses DeletedAtUtc directly (IsDeleted is a computed property)
        builder.HasQueryFilter(v => v.DeletedAtUtc == null);
    }
}
