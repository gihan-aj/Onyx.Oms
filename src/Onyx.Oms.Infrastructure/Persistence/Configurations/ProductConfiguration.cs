using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Onyx.Oms.Core.Domain.Entities;

namespace Onyx.Oms.Infrastructure.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(p => p.Description)
            .HasMaxLength(2000);

        builder.Property(p => p.Brand)
            .HasMaxLength(100);

        builder.Property(p => p.Material)
            .HasMaxLength(100);

        builder.Property(p => p.BasePrice)
            .HasPrecision(18, 2);

        builder.Property(p => p.BaseCost)
            .HasPrecision(18, 2);

        builder.Property(p => p.BaseWeight)
            .HasPrecision(18, 3); // 3 decimal places for kg/g

        builder.Property(p => p.Tags)
            .HasMaxLength(1000);

        // Tags - Stored as JSON
        builder.PrimitiveCollection(p => p.Tags)
            .ElementType()
            .HasMaxLength(50);

        // Relationships
        builder.HasOne(p => p.Category)
            .WithMany()
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.Variants)
            .WithOne(v => v.Product)
            .HasForeignKey(v => v.ProductId)
            .OnDelete(DeleteBehavior.Cascade); // Deleting product deletes variants

        builder.HasMany(p => p.Images)
            .WithOne(i => i.Product)
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
