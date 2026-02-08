using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Onyx.Oms.Core.Domain.Entities;

namespace Onyx.Oms.Infrastructure.Persistence.Configurations;

public class CourierConfiguration : IEntityTypeConfiguration<Courier>
{
    public void Configure(EntityTypeBuilder<Courier> builder)
    {
        builder.HasKey(c => c.Id);

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

        builder.Property(c => c.CreatedBy)
            .HasMaxLength(36);

        builder.Property(c => c.LastModifiedBy)
            .HasMaxLength(36)
            .IsRequired(false);

        builder.HasIndex(c => c.Name).IsUnique();
    }
}
