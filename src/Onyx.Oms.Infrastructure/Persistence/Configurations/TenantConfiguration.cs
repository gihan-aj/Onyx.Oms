using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Onyx.Oms.Core.Domain.Entities;

namespace Onyx.Oms.Infrastructure.Persistence.Configurations;

public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("Tenants");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.CompanyName).IsRequired().HasMaxLength(200);
        builder.Property(t => t.ContactEmail).IsRequired().HasMaxLength(200);
        builder.Property(t => t.ContactPhone).HasMaxLength(50);
        builder.Property(t => t.LegalName).HasMaxLength(200);
        builder.Property(t => t.TaxRegistrationNumber).HasMaxLength(100);

        builder.Property(t => t.DefaultCurrency).HasMaxLength(3).IsRequired();
        builder.Property(t => t.TimeZone).HasMaxLength(100).IsRequired();
        builder.Property(t => t.WeightUnit).HasMaxLength(20).IsRequired();

        builder.Property(t => t.InvoiceFooterText).HasMaxLength(1000);
        builder.Property(t => t.LogoUrl).HasMaxLength(500);
        builder.Property(t => t.HeroImage).HasMaxLength(500);

        builder.Property(t => t.PreferencesJson).HasColumnType("nvarchar(max)");

        builder.Property(c => c.CreatedBy).HasMaxLength(36);
        builder.Property(c => c.LastModifiedBy).HasMaxLength(36);

        builder.OwnsOne(t => t.StoreAddress, a =>
        {
            a.Property(p => p.Street).HasMaxLength(200).HasColumnName("Street");
            a.Property(p => p.City).HasMaxLength(100).HasColumnName("City");
            a.Property(p => p.State).HasMaxLength(100).HasColumnName("State");
            a.Property(p => p.PostalCode).HasMaxLength(20).HasColumnName("PostalCode");
            a.Property(p => p.Country).HasMaxLength(100).HasColumnName("Country");
        });

        var navigation = builder.Metadata.FindNavigation(nameof(Tenant.Users));
        navigation?.SetPropertyAccessMode(PropertyAccessMode.Field);

        // One-to-One relationship. Tenant is the principal, TenantSubscription is the dependent holding the FK (TenantId)
        builder.HasOne(t => t.Subscription)
            .WithOne()
            .HasForeignKey<TenantSubscription>(ts => ts.TenantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
