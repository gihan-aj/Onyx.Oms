using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Onyx.Oms.Core.Domain.Entities;

namespace Onyx.Oms.Infrastructure.Persistence.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");
        builder.HasKey(o => o.Id);

        builder.Property(o => o.TenantId)
            .IsRequired();

        builder.Property(o => o.OrderNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(o => o.OrderNumber).IsUnique();

        builder.Property(o => o.Notes)
            .HasMaxLength(4000);

        builder.Property(o => o.DeliveryInstructions)
            .HasMaxLength(500);

        builder.Property(o => o.TrackingNumber)
            .HasMaxLength(100);

        builder.Property(o => o.DiscountReason)
            .HasMaxLength(500);

        builder.OwnsOne(o => o.ShippingAddress, a =>
        {
            a.Property(p => p.Street).HasMaxLength(200).HasColumnName("ShippingStreet");
            a.Property(p => p.City).HasMaxLength(100).HasColumnName("ShippingCity");
            a.Property(p => p.District).HasMaxLength(100).HasColumnName("ShippingDistrict");
            a.Property(p => p.State).HasMaxLength(100).HasColumnName("ShippingState");
            a.Property(p => p.PostalCode).HasMaxLength(20).HasColumnName("ShippingPostalCode");
            a.Property(p => p.Country).HasMaxLength(100).HasColumnName("ShippingCountry");
        });

        builder.ComplexProperty(o => o.SubTotal, pb =>
        {
            pb.Property(m => m.Amount).HasColumnName("SubTotalAmount").HasPrecision(18, 2);
            pb.Property(m => m.Currency).HasColumnName("SubTotalCurrency").HasMaxLength(3);
        });

        builder.ComplexProperty(o => o.DiscountAmount, pb =>
        {
            pb.Property(m => m.Amount).HasColumnName("DiscountAmount").HasPrecision(18, 2);
            pb.Property(m => m.Currency).HasColumnName("DiscountCurrency").HasMaxLength(3);
        });

        builder.ComplexProperty(o => o.ShippingCost, pb =>
        {
            pb.Property(m => m.Amount).HasColumnName("ShippingCostAmount").HasPrecision(18, 2);
            pb.Property(m => m.Currency).HasColumnName("ShippingCostCurrency").HasMaxLength(3);
        });

        builder.ComplexProperty(o => o.TaxAmount, pb =>
        {
            pb.Property(m => m.Amount).HasColumnName("TaxAmount").HasPrecision(18, 2);
            pb.Property(m => m.Currency).HasColumnName("TaxCurrency").HasMaxLength(3);
        });

        builder.ComplexProperty(o => o.GrandTotal, pb =>
        {
            pb.Property(m => m.Amount).HasColumnName("GrandTotalAmount").HasPrecision(18, 2);
            pb.Property(m => m.Currency).HasColumnName("GrandTotalCurrency").HasMaxLength(3);
        });

        builder.Ignore(o => o.TotalPaid);
        builder.Ignore(o => o.BalanceAmount);

        // One-to-many relationship mapping
        builder.HasMany(o => o.Items)
            .WithOne(i => i.Order)
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(o => o.Payments)
            .WithOne()
            .HasForeignKey(p => p.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasIndex(o => o.TenantId);
    }
}
