using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Onyx.Oms.Core.Domain.Entities;
using System.Text.Json;

namespace Onyx.Oms.Infrastructure.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.TenantId)
            .IsRequired();

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.BaseSku)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(p => p.BaseSku).IsUnique();

        builder.Property(p => p.Description)
            .HasMaxLength(1000);

        // Money — non-nullable owned value objects; stored as inline columns via ComplexProperty (EF8+)
        builder.ComplexProperty(p => p.BasePrice, pb =>
        {
            pb.Property(m => m.Amount).HasColumnName("BasePriceAmount").HasPrecision(18, 2);
            pb.Property(m => m.Currency).HasColumnName("BasePriceCurrency").HasMaxLength(3);
        });

        builder.ComplexProperty(p => p.BaseCost, pb =>
        {
            pb.Property(m => m.Amount).HasColumnName("BaseCostAmount").HasPrecision(18, 2);
            pb.Property(m => m.Currency).HasColumnName("BaseCostCurrency").HasMaxLength(3);
        });

        // Weight — nullable; ComplexProperty cannot be null in EF8/9/10, so use OwnsOne instead.
        // All weight columns will be nullable in the DB to reflect the nullable CLR type.
        builder.OwnsOne(p => p.BaseWeight, wb =>
        {
            wb.Property(w => w.Value).HasColumnName("BaseWeightValue").HasPrecision(10, 3);
            wb.Property(w => w.Unit).HasColumnName("BaseWeightUnit").HasMaxLength(10);
        });

        // Options — List<ProductOption> stored as a JSON column (EF8+ ToJson)
        builder.OwnsMany(p => p.Options, ob =>
        {
            ob.ToJson();
            ob.Property(o => o.Name).HasMaxLength(100);
        });

        // Tags — List<string> stored as a JSON column
        builder.PrimitiveCollection(p => p.Tags)
            .HasColumnType("nvarchar(max)")
            .HasColumnName("Tags");

        // Specifications — Dictionary<string, string>; no native ToJson() for dict properties in EF10.
        // We target the private backing field (_specifications : Dictionary<string, string>) explicitly
        // so EF Core sees the concrete type, not the IReadOnlyDictionary<> interface exposed by the getter.
        // This makes HasConversion and HasValueComparer work without any type-mismatch issues.
        var specsComparer = new ValueComparer<IReadOnlyDictionary<string, string>>(
            (a, b) => a != null && b != null && a.Count == b.Count && !a.Except(b).Any(),
            d => d.Aggregate(0, (acc, kvp) => HashCode.Combine(acc, kvp.Key.GetHashCode(), kvp.Value.GetHashCode())),
            d => new Dictionary<string, string>(d));

        builder.Property(p => p.Specifications)
            .HasField("_specifications")
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, (JsonSerializerOptions?)null) ?? new(), specsComparer)
            .HasColumnType("nvarchar(max)")
            .HasColumnName("Specifications");

        // Relations
        builder.HasMany(p => p.Variants)
            .WithOne(v => v.Product)
            .HasForeignKey(v => v.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Images)
            .WithOne(i => i.Product)
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.Category)
            .WithMany()
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => p.TenantId);
    }
}
