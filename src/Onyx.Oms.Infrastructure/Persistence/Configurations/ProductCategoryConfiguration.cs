using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Onyx.Oms.Core.Domain.Entities;

namespace Onyx.Oms.Infrastructure.Persistence.Configurations;

public class ProductCategoryConfiguration : IEntityTypeConfiguration<ProductCategory>
{
    public void Configure(EntityTypeBuilder<ProductCategory> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.TenantId)
            .IsRequired();

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Description)
            .HasMaxLength(500);

        builder.Property(c => c.Path)
            .HasMaxLength(500); // Indexing this is good for hierarchy queries
            
        builder.Property(c => c.NamePath)
            .HasMaxLength(1000);

        builder.Property(c => c.IconUrl)
            .HasMaxLength(255);

        builder.Property(c => c.Color)
            .HasMaxLength(20);

        builder.OwnsMany(c => c.Specifications, sb =>
        {
            sb.ToJson(); // Maps to a single database column in the database
        }); 

        // Self-referencing relationship
        builder.HasOne(c => c.ParentCategory)
            .WithMany(c => c.SubCategories)
            .HasForeignKey(c => c.ParentCategoryId)
            .OnDelete(DeleteBehavior.Restrict); // Prevent cascading delete to avoid accidental tree wiping

        // Indexes
        builder.HasIndex(c => new {c.TenantId, c.Path});
        builder.HasIndex(c => c.ParentCategoryId);
    }
}
