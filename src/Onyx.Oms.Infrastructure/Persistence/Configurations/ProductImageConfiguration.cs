using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Onyx.Oms.Core.Domain.Entities;

namespace Onyx.Oms.Infrastructure.Persistence.Configurations;

public class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
{
    public void Configure(EntityTypeBuilder<ProductImage> builder)
    {
        builder.ToTable("ProductImages");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.TenantId)
            .IsRequired();

        builder.Property(i => i.Url)
            .IsRequired()
            .HasMaxLength(2048);

        // Option linking for smart image-to-variant filtering
        builder.Property(i => i.OptionName)
            .HasMaxLength(100);

        builder.Property(i => i.OptionValue)
            .HasMaxLength(200);

        // Index for efficient filtering: e.g. "give me all images for Color: Red"
        builder.HasIndex(i => new { i.TenantId, i.ProductId, i.OptionName, i.OptionValue });
    }
}
