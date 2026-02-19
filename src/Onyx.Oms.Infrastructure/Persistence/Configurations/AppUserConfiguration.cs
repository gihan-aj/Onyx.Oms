using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Onyx.Oms.Core.Domain.Entities;

namespace Onyx.Oms.Infrastructure.Persistence.Configurations;

public class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> builder)
    {
        builder.ToTable("AppUsers");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.IdentityUserId)
            .HasMaxLength(36)
            .IsRequired();

        builder.Property(x => x.Email)
            .HasMaxLength(200)
            .IsRequired();
        
        builder.HasIndex(x => x.Email).IsUnique();

        builder.Property(x => x.FirstName).HasMaxLength(100);
        builder.Property(x => x.LastName).HasMaxLength(100);

        builder.Property(c => c.CreatedBy)
            .HasMaxLength(36);

        builder.Property(c => c.LastModifiedBy)
            .HasMaxLength(36)
            .IsRequired(false);

        // Many-to-Many Relationship Configuration
        builder.HasMany(x => x.Roles)
            .WithMany(x => x.Users)
            .UsingEntity(j => j.ToTable("AppUserRoles"));
    }
}
