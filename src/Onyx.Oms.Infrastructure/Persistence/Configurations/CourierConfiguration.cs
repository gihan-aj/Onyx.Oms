using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Onyx.Oms.Core.Domain.Entities;
using Onyx.Oms.Core.Domain.Enums;

namespace Onyx.Oms.Infrastructure.Persistence.Configurations;

public class CourierConfiguration : IEntityTypeConfiguration<Courier>
{
    public void Configure(EntityTypeBuilder<Courier> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(x => x.TenantId)
            .IsRequired();

        builder.Property(c => c.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(c => c.ContactPerson)
            .HasMaxLength(200);

        builder.Property(c => c.PrimaryPhone)
            .HasMaxLength(50);

        builder.Property(c => c.SecondaryPhone)
            .HasMaxLength(50);

        builder.Property(c => c.WebsiteUrl)
            .HasMaxLength(500);

        builder.Property(c => c.TrackingUrlTemplate)
            .HasMaxLength(500);

        builder.Property(c => c.ProviderType)
            .IsRequired()
            .HasDefaultValue(CourierProviderType.StandardCustom);

        builder.Property(c => c.IsSystemManaged)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasMany(c => c.ZoneRates)
            .WithOne(zr => zr.Courier)
            .HasForeignKey(zr => zr.CourierId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.TenantId);
        builder.HasIndex(c => new { c.TenantId, c.Name })
            .IsUnique();
    }
}

public class CourierZoneRateConfiguration : IEntityTypeConfiguration<CourierZoneRate>
{
    public void Configure(EntityTypeBuilder<CourierZoneRate> builder)
    {
        builder.ToTable("CourierZoneRates");

        builder.HasKey(c => c.Id);

        builder.Property(x => x.TenantId)
            .IsRequired();

        builder.Property(c => c.ZoneName)
            .HasMaxLength(200)
            .IsRequired();

        builder.ComplexProperty(p => p.BaseFee, pb =>
        {
            pb.Property(m => m.Amount).HasColumnName("BaseFeeAmount").HasPrecision(18, 2);
            pb.Property(m => m.Currency).HasColumnName("BaseFeeCurrency").HasMaxLength(3);
        });

        builder.OwnsOne(p => p.BaseWeight, wb =>
        {
            wb.Property(w => w.Value).HasColumnName("BaseWeightValue").HasPrecision(10, 3);
            wb.Property(w => w.Unit).HasColumnName("BaseWeightUnit").HasMaxLength(10);
        });

        builder.ComplexProperty(p => p.ExcessFeePerWeightUnit, pb =>
        {
            pb.Property(m => m.Amount).HasColumnName("ExcessFeePerWeightUnitAmount").HasPrecision(18, 2);
            pb.Property(m => m.Currency).HasColumnName("ExcessFeePerWeightUnitCurrency").HasMaxLength(3);
        });

        builder.Property(c => c.CoveredDistrics)
            .HasColumnType("json");

        builder.Property(c => c.CodPercentage)
            .HasPrecision(5, 2)
            .IsRequired();

        builder.HasIndex(x => x.TenantId);
    }
}
