using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using norviguet_control_fletes_api.Models.Entities;

namespace norviguet_control_fletes_api.Data.Configurations
{
    public class SellerConfiguration : IEntityTypeConfiguration<Seller>
    {
        public void Configure(EntityTypeBuilder<Seller> builder)
        {
            // Relationships are configured in Order entity
        }
    }
}
