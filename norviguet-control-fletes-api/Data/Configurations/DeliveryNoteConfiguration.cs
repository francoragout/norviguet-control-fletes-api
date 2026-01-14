using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using norviguet_control_fletes_api.Models.Entities;

namespace norviguet_control_fletes_api.Data.Configurations
{
    public class DeliveryNoteConfiguration : IEntityTypeConfiguration<DeliveryNote>
    {
        public void Configure(EntityTypeBuilder<DeliveryNote> builder)
        {
            builder.Property(dn => dn.Status)
                .HasConversion<string>();

            builder.HasIndex(dn => dn.DeliveryNoteNumber)
                .IsUnique();

            builder.HasOne(dn => dn.Carrier)
                .WithMany(c => c.DeliveryNotes)
                .HasForeignKey(dn => dn.CarrierId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
