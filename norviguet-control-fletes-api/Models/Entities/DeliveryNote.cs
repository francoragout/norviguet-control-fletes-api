using norviguet_control_fletes_api.Models.Enums;

namespace norviguet_control_fletes_api.Models.Entities
{
    public class DeliveryNote : AuditableEntity
    {
        public DeliveryNoteStatus Status { get; set; } = DeliveryNoteStatus.Pending;
        public string DeliveryNoteNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;

        // Foreign Keys
        public int OrderId { get; set; }
        public int CarrierId { get; set; }

        // Relationships
        public required Order Order { get; set; }
        public required Carrier Carrier { get; set; }
    }
}
