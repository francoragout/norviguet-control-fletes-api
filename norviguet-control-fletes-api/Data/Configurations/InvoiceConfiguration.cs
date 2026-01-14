using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using norviguet_control_fletes_api.Models.Entities;

namespace norviguet_control_fletes_api.Data.Configurations
{
    public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
    {
        public void Configure(EntityTypeBuilder<Invoice> builder)
        {
            builder.Property(i => i.Type)
                .HasConversion<string>();

            builder.Property(i => i.Price)
                .HasPrecision(18, 2);

            builder.HasIndex(i => i.InvoiceNumber)
                .IsUnique();

            builder.HasIndex(i => new { i.OrderId, i.CarrierId })
                .IsUnique();

            builder.HasOne(i => i.Carrier)
                .WithMany(c => c.Invoices)
                .HasForeignKey(i => i.CarrierId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
