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

        builder.HasOne(x => x.Tenant)
            .WithMany(t => t.Users)
            .HasForeignKey(x => x.TenantId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.Email)
            .HasMaxLength(200)
            .IsRequired();
        
        builder.HasIndex(x => x.Email).IsUnique();

        builder.Property(x => x.FirstName).HasMaxLength(100);
        builder.Property(x => x.LastName).HasMaxLength(100);

        // Many-to-Many Relationship Configuration
        builder.HasMany(x => x.Roles)
            .WithMany(x => x.Users)
            .UsingEntity(j => j.ToTable("AppUserRoles"));
    }
}
