using norviguet_control_fletes_api.Models.Enums;

namespace norviguet_control_fletes_api.Models.Entities
{
    public class Invoice : AuditableEntity
    {
        public InvoiceType Type { get; set; } = InvoiceType.A;
        public string InvoiceNumber { get; set; } = string.Empty;
        public int Price { get; set; }

        // Foreign Keys
        public int CarrierId { get; set; }
        public int OrderId { get; set; }

        // Relationships
        public required Order Order { get; set; }
        public required Carrier Carrier { get; set; }
    }
}
