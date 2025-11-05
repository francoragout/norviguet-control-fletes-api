namespace norviguet_control_fletes_api.Entities
{
    public class PaymentOrder
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string PaymentOrderNumber { get; set; } = string.Empty;
        public int InvoiceId { get; set; }
        public int OrderId { get; set; }

        // Relationships
        public required Invoice Invoice { get; set; }
        public required Order Order { get; set; }
    }
}
