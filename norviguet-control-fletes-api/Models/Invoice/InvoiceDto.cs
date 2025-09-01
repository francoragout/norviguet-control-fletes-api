namespace norviguet_control_fletes_api.Models.Invoice
{
    public class InvoiceDto
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Type { get; set; } = string.Empty;
        public int PointOfSale { get; set; }
    }
}