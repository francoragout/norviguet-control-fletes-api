namespace norviguet_control_fletes_api.Entities
{
    public class PaymentOrder
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string PointOfSale { get; set; } = string.Empty;
        public string PaymentOrderNumber { get; set; } = string.Empty;

        // Relationships
        public required Invoice Invoice { get; set; }
        public int InvoiceId { get; set; }
    }
}
