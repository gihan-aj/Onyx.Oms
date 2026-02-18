using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Onyx.Oms.Core.Domain.Entities;

namespace Onyx.Oms.Infrastructure.Persistence.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.HasKey(r => r.Id); // Guid ID

        builder.Property(r => r.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.HasIndex(r => r.Name)
            .IsUnique();

        builder.Property(r => r.Description)
            .HasMaxLength(500);

        builder.Property(u => u.Permissions)
            .HasMaxLength(1000);

        // JSON Storage for Permissions
        builder.PrimitiveCollection(r => r.Permissions)
            .ElementType()
            .HasMaxLength(100);

        builder.Property(c => c.CreatedBy)
            .HasMaxLength(36);

        builder.Property(c => c.LastModifiedBy)
            .HasMaxLength(36)
            .IsRequired(false);
    }
}
