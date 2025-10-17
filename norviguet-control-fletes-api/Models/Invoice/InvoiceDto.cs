namespace norviguet_control_fletes_api.Models.Invoice
{
    public class InvoiceDto
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Type { get; set; } = string.Empty;
        public string PointOfSale { get; set; } = string.Empty;
        public string Number { get; set; } = string.Empty;
        public List<int> OrderIds { get; set; } = new();
    }
}