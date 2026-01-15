namespace norviguet_control_fletes_api.Models.Entities
{
    public class PaymentOrder : AuditableEntity
    {
        public string PaymentOrderNumber { get; set; } = string.Empty;

        // Foreign Keys
        public int OrderId { get; set; }
        public int CarrierId { get; set; }

        // Relationships
        public required Order Order { get; set; }
        public required Carrier Carrier { get; set; }
    }
}
