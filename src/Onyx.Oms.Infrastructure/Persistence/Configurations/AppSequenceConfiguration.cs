using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Onyx.Oms.Infrastructure.Persistence.Entities;

namespace Onyx.Oms.Infrastructure.Persistence.Configurations;

public class AppSequenceConfiguration : IEntityTypeConfiguration<AppSequence>
{
    public void Configure(EntityTypeBuilder<AppSequence> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Id)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(x => x.CurrentValue)
            .IsRequired();
    }
}
