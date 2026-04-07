using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Onyx.Oms.Core.Domain.Entities;

namespace Onyx.Oms.Infrastructure.Persistence.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.TenantId)
            .IsRequired();

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(200);

        // Email is optional but unique if present
        builder.HasIndex(c => c.Email)
            .IsUnique()
            .HasFilter("[Email] IS NOT NULL");

        builder.Property(c => c.Email)
            .IsRequired(false)
            .HasMaxLength(200);

        builder.Property(c => c.PrimaryPhone)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(c => c.SecondaryPhone)
            .HasMaxLength(50);

        builder.Property(c => c.LastOrderNumber)
            .HasMaxLength(20)
            .IsRequired(false);

        builder.Property(c => c.Notes)
            .HasMaxLength(1000);

        builder.Property(c => c.IsActive)
            .HasDefaultValue(true);

        // Configuring the Value Object
        builder.OwnsOne(c => c.Address, a =>
        {
            a.Property(p => p.Street).HasMaxLength(200).HasColumnName("Street");
            a.Property(p => p.City).HasMaxLength(100).HasColumnName("City");
            a.Property(p => p.District).HasMaxLength(100).HasColumnName("District");
            a.Property(p => p.State).HasMaxLength(100).HasColumnName("State");
            a.Property(p => p.PostalCode).HasMaxLength(20).HasColumnName("PostalCode");
            a.Property(p => p.Country).HasMaxLength(100).HasColumnName("Country");
        });

        builder.HasIndex(x => new { x.TenantId, x.IsActive });
    }
}
