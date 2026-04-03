using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Onyx.Oms.Core.Domain.Entities;

namespace Onyx.Oms.Infrastructure.Persistence.Configurations;

//public class TenantProfileConfiguration : IEntityTypeConfiguration<TenantProfile>
//{
//    public void Configure(EntityTypeBuilder<TenantProfile> builder)
//    {
//        builder.ToTable("TenantProfiles");
//        builder.HasKey(t => t.Id);

//        builder.Property(t => t.StoreName).IsRequired().HasMaxLength(200);
//        builder.Property(t => t.LegalName).HasMaxLength(200);
//        builder.Property(t => t.TaxRegistrationNumber).HasMaxLength(100);
//        builder.Property(t => t.ContactEmail).IsRequired().HasMaxLength(255);
//        builder.Property(t => t.ContactPhone).HasMaxLength(50);
        
//        builder.Property(t => t.BaseCurrency).IsRequired().HasMaxLength(3);
//        builder.Property(t => t.WeightUnit).IsRequired().HasMaxLength(10);
        
//        builder.Property(t => t.InvoiceFooterText).HasMaxLength(1000);
//        builder.Property(t => t.LogoUrl).HasMaxLength(2000);

//        // Maps the JSON string
//        builder.Property(t => t.PreferencesJson).HasColumnType("nvarchar(max)");

//        // Map the Address Value Object
//        builder.ComplexProperty(t => t.StoreAddress, ab => 
//        {
//            ab.IsRequired(false);
//            ab.Property(a => a.Street).HasColumnName("AddressStreet").HasMaxLength(200);
//            ab.Property(a => a.City).HasColumnName("AddressCity").HasMaxLength(100);
//            ab.Property(a => a.State).HasColumnName("AddressState").HasMaxLength(100);
//            ab.Property(a => a.PostalCode).HasColumnName("AddressPostalCode").HasMaxLength(20);
//            ab.Property(a => a.Country).HasColumnName("AddressCountry").HasMaxLength(100);
//        });

//        builder.Property(c => c.CreatedBy)
//            .HasMaxLength(36);

//        builder.Property(c => c.LastModifiedBy)
//            .HasMaxLength(36)
//            .IsRequired(false);
//    }
//}
