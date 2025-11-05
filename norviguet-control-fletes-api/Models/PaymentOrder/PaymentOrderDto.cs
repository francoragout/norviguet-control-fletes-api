namespace norviguet_control_fletes_api.Models.Payment
{
    public class PaymentOrderDto
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string PaymentOrderNumber { get; set; } = string.Empty;

        // Foreign Keys
        public int InvoiceId { get; set; }
        public int OrderId { get; set; }
    }
}
