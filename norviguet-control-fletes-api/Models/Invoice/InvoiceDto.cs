namespace norviguet_control_fletes_api.Models.Invoice
{
    public class InvoiceDto
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Type { get; set; } = string.Empty;
        public string PointOfSale { get; set; } = string.Empty;
        public string InvoiceNumber { get; set; } = string.Empty;
        public int Price { get; set; }
        public int OrderId { get; set; }

        // Additional related data
        public string OrderNumber { get; set; } = string.Empty;
    }
}