namespace norviguet_control_fletes_api.Entities
{
    public enum DeliveryNoteStatus
    {
        Pending,
        Cancelled,
        Approved,
    }

    public class DeliveryNote
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DeliveryNoteStatus Status { get; set; } = DeliveryNoteStatus.Pending;
        public string DeliveryNoteNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;

        // Foreign Keys
        public int CarrierId { get; set; }
        public int OrderId { get; set; }

        // Relationships
        public required Carrier Carrier { get; set; }
        public Order Order { get; set; } = null!;
    }
}
