//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Metadata.Builders;
//using Onyx.Oms.Core.Domain.Entities;

//namespace Onyx.Oms.Infrastructure.Persistence.Configurations;

//public class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
//{
//    public void Configure(EntityTypeBuilder<ProductImage> builder)
//    {
//        builder.ToTable("ProductImages");
//        builder.HasKey(i => i.Id);

//        builder.Property(i => i.Url)
//            .IsRequired()
//            .HasMaxLength(2048);

//        builder.Property(i => i.Color)
//            .IsRequired(false)
//            .HasMaxLength(50);
//    }
//}
