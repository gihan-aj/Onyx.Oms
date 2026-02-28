using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Onyx.Oms.Core.Domain.Entities;

namespace Onyx.Oms.Infrastructure.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");
        builder.HasKey(p => p.Id);
        
        builder.Property(p => p.Name).IsRequired().HasMaxLength(200);
        builder.Property(p => p.BaseSku).IsRequired().HasMaxLength(100);
        builder.Property(p => p.Description).HasMaxLength(1000);
        builder.Property(p => p.Brand).HasMaxLength(100);
        builder.Property(p => p.Material).HasMaxLength(100);

        builder.Property(p => p.Tags).HasColumnName("Tags");

        // Structure Flags
        builder.Property(p => p.HasColor).IsRequired();
        builder.Property(p => p.HasSize).IsRequired();

        builder.ComplexProperty(p => p.BasePrice, pb => {
            pb.Property(m => m.Amount).HasColumnName("BasePriceAmount").HasPrecision(18, 2);
            pb.Property(m => m.Currency).HasColumnName("BasePriceCurrency").HasMaxLength(3);
        });

        builder.ComplexProperty(p => p.BaseCost, cb => {
            cb.Property(m => m.Amount).HasColumnName("BaseCostAmount").HasPrecision(18, 2);
            cb.Property(m => m.Currency).HasColumnName("BaseCostCurrency").HasMaxLength(3);
        });

        builder.ComplexProperty(p => p.BaseWeight, wb => {
            wb.Property(w => w.Value).HasColumnName("BaseWeightValue").HasPrecision(10, 3);
            wb.Property(w => w.Unit).HasColumnName("BaseWeightUnit").HasMaxLength(10);
        });

        builder.Property(c => c.CreatedBy)
            .HasMaxLength(36);

        builder.Property(c => c.LastModifiedBy)
             .HasMaxLength(36);

        builder.HasMany(p => p.Variants).WithOne(v => v.Product).HasForeignKey(v => v.ProductId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(p => p.Images).WithOne(i => i.Product).HasForeignKey(i => i.ProductId).OnDelete(DeleteBehavior.Cascade);
    }
}
