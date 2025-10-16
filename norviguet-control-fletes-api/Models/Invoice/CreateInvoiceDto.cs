using norviguet_control_fletes_api.Entities;
using System.ComponentModel.DataAnnotations;

namespace norviguet_control_fletes_api.Models.Invoice
{
    public class CreateInvoiceDto
    {
        [Required]
        public InvoiceType Type { get; set; }
        [Required]
        [StringLength(50, ErrorMessage = "The PointOfSale must be between 1 and 50 characters long.")]
        public string PointOfSale { get; set; } = string.Empty;
    }
}