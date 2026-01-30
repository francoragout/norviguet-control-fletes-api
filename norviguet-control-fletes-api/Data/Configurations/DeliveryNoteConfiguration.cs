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
                .IsRequired()
                .HasConversion<string>();

            builder.Property(dn => dn.DeliveryNoteNumber)
                .IsRequired()
                .HasMaxLength(14);

            builder.HasIndex(dn => dn.DeliveryNoteNumber)
                .IsUnique();

            builder.Property(dn => dn.Address)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(dn => dn.Location)
                .IsRequired()
                .HasMaxLength(100);

            builder.HasOne(dn => dn.Carrier)
                .WithMany(c => c.DeliveryNotes)
                .HasForeignKey(dn => dn.CarrierId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
