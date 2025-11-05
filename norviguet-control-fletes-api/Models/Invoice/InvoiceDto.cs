namespace norviguet_control_fletes_api.Models.Invoice
{
    public class InvoiceDto
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Type { get; set; } = string.Empty;
        public string InvoiceNumber { get; set; } = string.Empty;
        public int Price { get; set; }

        // Foreign Keys
        public int OrderId { get; set; }
        public int CarrierId { get; set; }

        // Additional related data
        public string CarrierName { get; set; } = string.Empty; 
    }
}