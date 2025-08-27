namespace norviguet_control_fletes_api.Models.Invoice
{
    public class DeleteInvoicesDto
    {
        public List<int> InvoiceIds { get; set; } = new();
    }
}