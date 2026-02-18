using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Onyx.Oms.Core.Domain.Entities;

namespace Onyx.Oms.Infrastructure.Persistence.Configurations;

public class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> builder)
    {
        builder.HasKey(u => u.Id); // Int ID

        builder.Property(u => u.IdentityUserId)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(u => u.IdentityUserId)
            .IsUnique();

        builder.Property(u => u.Email)
            .HasMaxLength(200)
            .IsRequired();
            
        builder.HasIndex(u => u.Email);

        builder.Property(u => u.DisplayName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(u => u.Roles)
            .HasMaxLength(1000);

        // JSON Storage for Roles
        builder.PrimitiveCollection(u => u.Roles)
            .ElementType()
            .HasMaxLength(100);

        builder.Property(c => c.CreatedBy)
            .HasMaxLength(36);

        builder.Property(c => c.LastModifiedBy)
            .HasMaxLength(36)
            .IsRequired(false);
    }
}
