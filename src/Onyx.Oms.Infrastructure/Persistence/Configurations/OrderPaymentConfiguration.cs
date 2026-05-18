using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Onyx.Oms.Core.Domain.Entities;

namespace Onyx.Oms.Infrastructure.Persistence.Configurations;

public class OrderPaymentConfiguration : IEntityTypeConfiguration<OrderPayment>
{
    public void Configure(EntityTypeBuilder<OrderPayment> builder)
    {
        builder.ToTable("OrderPayments");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.TenantId)
            .IsRequired();

        builder.Property(p => p.Reference)
            .HasMaxLength(200);

        builder.Property(p => p.Note)
            .HasMaxLength(200);

        builder.Property(p => p.GatewayName)
            .HasMaxLength(100);

        builder.Property(p => p.GatewayTransactionId)
            .HasMaxLength(200);

        builder.Property(p => p.GatewayPaymentStatus)
            .HasMaxLength(100);

        builder.ComplexProperty(p => p.Amount, pb =>
        {
            pb.Property(m => m.Amount).HasColumnName("PaymentAmount").HasPrecision(18, 2);
            pb.Property(m => m.Currency).HasColumnName("PaymentCurrency").HasMaxLength(3);
        });

        builder.ComplexProperty(p => p.GatewayFee, pb =>
        {
            pb.Property(m => m.Amount).HasColumnName("GatewayFeeAmount").HasPrecision(18, 2);
            pb.Property(m => m.Currency).HasColumnName("GatewayFeeCurrency").HasMaxLength(3);
        });

        builder.ComplexProperty(p => p.Received, pb =>
        {
            pb.Property(m => m.Amount).HasColumnName("ReceivedAmount").HasPrecision(18, 2);
            pb.Property(m => m.Currency).HasColumnName("ReceivedFeeCurrency").HasMaxLength(3);
        });

        builder.HasIndex(p => p.TenantId);
    }
}
