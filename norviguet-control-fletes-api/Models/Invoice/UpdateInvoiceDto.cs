namespace norviguet_control_fletes_api.Models.Invoice
{
    public class UpdateInvoiceDto
    {
        public string? Type { get; set; }
        public int? PointOfSale { get; set; }
        public int? Number { get; set; }
    }
}