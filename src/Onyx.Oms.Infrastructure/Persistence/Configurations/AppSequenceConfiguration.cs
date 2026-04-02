using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Onyx.Oms.Infrastructure.Persistence.Entities;

namespace Onyx.Oms.Infrastructure.Persistence.Configurations;

public class AppSequenceConfiguration : IEntityTypeConfiguration<AppSequence>
{
    public void Configure(EntityTypeBuilder<AppSequence> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.TenantId)
            .IsRequired();
        
        builder.Property(x => x.Prefix)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(x => x.CurrentValue)
            .IsRequired();

        builder.Property(x => x.RowVersion)
            .IsRowVersion();

        builder.HasIndex(x => new { x.TenantId, x.Prefix }).IsUnique();
    }
}
