using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using norviguet_control_fletes_api.Models.Entities;

namespace norviguet_control_fletes_api.Data.Configurations
{
    public class PaymentOrderConfiguration : IEntityTypeConfiguration<PaymentOrder>
    {
        public void Configure(EntityTypeBuilder<PaymentOrder> builder)
        {
            builder.HasIndex(po => po.Number)
                .IsUnique();

            builder.HasIndex(po => po.Number)
                .IsUnique();

            builder.HasIndex(po => new { po.OrderId, po.CarrierId })
                .IsUnique();

            builder.Property(c => c.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

            builder.HasOne(po => po.Carrier)
                .WithMany(c => c.PaymentOrders)
                .HasForeignKey(po => po.CarrierId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
