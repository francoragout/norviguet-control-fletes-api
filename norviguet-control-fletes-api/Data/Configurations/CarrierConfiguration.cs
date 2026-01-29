using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using norviguet_control_fletes_api.Models.Entities;

namespace norviguet_control_fletes_api.Data.Configurations
{
    public class CarrierConfiguration : IEntityTypeConfiguration<Carrier>
    {
        public void Configure(EntityTypeBuilder<Carrier> builder)
        {
            builder.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(50);

            builder.HasIndex(c => c.Name)
                .IsUnique();

            builder.Property(c => c.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
        }
    }
}
