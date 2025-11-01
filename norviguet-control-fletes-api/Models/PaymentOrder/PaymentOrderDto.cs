namespace norviguet_control_fletes_api.Models.Payment
{
    public class PaymentOrderDto
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string PointOfSale { get; set; } = string.Empty;
        public string PaymentOrderNumber { get; set; } = string.Empty;
        public int InvoiceId { get; set; }

        // Additional related data
        public string InvoiceNumber { get; set; } = string.Empty;
    }
}
