namespace norviguet_control_fletes_api.Entities
{
    public enum InvoiceType
    {
        A,
        B,
        C,
        Z
    }

    public class Invoice
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public InvoiceType Type { get; set; } = InvoiceType.A;
        public string InvoiceNumber { get; set; } = string.Empty;
        public int Price { get; set; }

        // Foreign Keys
        public int CarrierId { get; set; }
        public int OrderId { get; set; }

        // Relationships
        public required Carrier Carrier { get; set; }
        public required Order Order { get; set; }
        public PaymentOrder? PaymentOrder { get; set; }
    }
}
