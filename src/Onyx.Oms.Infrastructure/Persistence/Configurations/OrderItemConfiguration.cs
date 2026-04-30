using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Onyx.Oms.Core.Domain.Entities;

namespace Onyx.Oms.Infrastructure.Persistence.Configurations;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItems");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.TenantId)
            .IsRequired();

        builder.Property(p => p.ProductName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(v => v.Sku)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(i => i.DiscountReason)
            .HasMaxLength(500);

        builder.ComplexProperty(i => i.UnitPrice, pb =>
        {
            pb.Property(m => m.Amount).HasColumnName("UnitPriceAmount").HasPrecision(18, 2);
            pb.Property(m => m.Currency).HasColumnName("UnitPriceCurrency").HasMaxLength(3);
        });

        builder.ComplexProperty(i => i.LineTotal, pb =>
        {
            pb.Property(m => m.Amount).HasColumnName("LineTotalAmount").HasPrecision(18, 2);
            pb.Property(m => m.Currency).HasColumnName("LineTotalCurrency").HasMaxLength(3);
        });

        builder.ComplexProperty(i => i.DiscountAmount, pb =>
        {
            pb.Property(m => m.Amount).HasColumnName("DiscountAmount").HasPrecision(18, 2);
            pb.Property(m => m.Currency).HasColumnName("DiscountCurrency").HasMaxLength(3);
        });

        builder.HasIndex(i => i.TenantId);
    }
}
