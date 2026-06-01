using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Entities;

namespace Onyx.Oms.Infrastructure.Persistence.Configurations
{
    public class ExpenseConfiguration : IEntityTypeConfiguration<Expense>
    {
        public void Configure(EntityTypeBuilder<Expense> builder)
        {
            builder.ToTable("Expenses");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.TenantId)
                .IsRequired();

            builder.Property(c => c.Category)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(c => c.Reference)
                .HasMaxLength(500);

            builder.Property(c => c.Notes)
                .HasMaxLength(500);

            builder.Ignore(v => v.IsDeleted);

            builder.ComplexProperty(v => v.Amount, pb =>
            {
                pb.Property(m => m.Amount).HasColumnName("Amount").HasPrecision(18, 2);
                pb.Property(m => m.Currency).HasColumnName("Currency").HasMaxLength(3);
            });

            builder.HasIndex(o => o.TenantId);
            builder.HasIndex(e => new { e.TenantId, e.Category })
                .HasDatabaseName("IX_Expenses_TenantId_Category");

            builder.HasQueryFilter(v => v.DeletedAtUtc == null);
        }
    }
}
