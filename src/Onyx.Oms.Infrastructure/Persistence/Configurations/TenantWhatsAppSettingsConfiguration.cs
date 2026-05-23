using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Onyx.Oms.Core.Domain.Entities;

namespace Onyx.Oms.Infrastructure.Persistence.Configurations;

public class TenantWhatsAppSettingsConfiguration : IEntityTypeConfiguration<TenantWhatsAppSettings>
{
    public void Configure(EntityTypeBuilder<TenantWhatsAppSettings> builder)
    {
        builder.ToTable("TenantWhatsAppSettings");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.PhoneNumberId).IsRequired().HasMaxLength(500);
        builder.Property(t => t.EncryptedAccessToken).IsRequired().HasMaxLength(2000);
    }
}