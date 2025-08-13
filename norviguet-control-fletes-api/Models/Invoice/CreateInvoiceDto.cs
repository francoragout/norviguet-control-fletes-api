namespace norviguet_control_fletes_api.Models.Invoice
{
    public class CreateInvoiceDto
    {
        public string Type { get; set; } = string.Empty;
        public int PointOfSale { get; set; }
        public int Number { get; set; }
    }
}