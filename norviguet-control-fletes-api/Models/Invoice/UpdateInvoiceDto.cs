using norviguet_control_fletes_api.Entities;
using System.ComponentModel.DataAnnotations;

namespace norviguet_control_fletes_api.Models.Invoice
{
    public class UpdateInvoiceDto
    {
        [Required]
        public InvoiceType Type { get; set; } = InvoiceType.A;
        [Required]
        [RegularExpression("^\\d{5}-\\d{8}$", ErrorMessage = "InvoiceNumber must have the format NNNNN-NNNNNNNN")]
        public string InvoiceNumber { get; set; } = string.Empty;
        [Required]
        public int Price { get; set; }
        [Required]
        public int OrderId { get; set; }
        [Required]
        public int CarrierId { get; set; }
    }
}