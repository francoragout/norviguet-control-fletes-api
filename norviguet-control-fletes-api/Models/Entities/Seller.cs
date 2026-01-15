using norviguet_control_fletes_api.Models.Commons;

namespace norviguet_control_fletes_api.Models.Entities
{
    public class Seller : AuditableEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Zone { get; set; }

        // Relationships
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
