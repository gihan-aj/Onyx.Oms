using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Onyx.Oms.Core.Domain.Entities;

namespace Onyx.Oms.Infrastructure.Persistence.Configurations
{
    public class FulFillmentTaskConfiguration : IEntityTypeConfiguration<FulfillmentTask>
    {
        public void Configure(EntityTypeBuilder<FulfillmentTask> builder)
        {
            builder.ToTable("FulFillmentTasks");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.TenantId)
                .IsRequired();

            builder.Property(t => t.Type)
               .HasConversion<string>()
               .HasMaxLength(20)
               .IsRequired();

            builder.Property(t => t.Status)
                   .HasConversion<string>()
                   .HasMaxLength(20)
                   .IsRequired();

            builder.Property(t => t.Priority)
                   .HasConversion<string>()
                   .HasMaxLength(20)
                   .IsRequired();

            builder.Property(t => t.RequestedQuantity).IsRequired();
            builder.Property(t => t.StartedQuantity).IsRequired();
            builder.Property(t => t.CompletedQuantity).IsRequired();
            builder.Property(t => t.ScrappedQuantity).IsRequired();

            builder.Property(t => t.PurchaseOrderNumber)
               .HasMaxLength(100);

            builder.Property(t => t.Notes)
                   .HasMaxLength(1000);

            builder.OwnsOne(t => t.Cost, costBuilder =>
            {
                costBuilder.Property(c => c.Amount)
                           .HasColumnName("CostAmount")
                           .HasPrecision(18, 2)
                           .IsRequired();

                costBuilder.Property(c => c.Currency)
                           .HasColumnName("CostCurrency")
                           .HasMaxLength(3)
                           .IsRequired();
            });

            builder.HasIndex(t => t.ProductVariantId);
            builder.HasIndex(t => t.TenantId);
        }
    }
}
