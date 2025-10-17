using norviguet_control_fletes_api.Entities;
using System.ComponentModel.DataAnnotations;

namespace norviguet_control_fletes_api.Models.Invoice
{
    public class UpdateInvoiceDto
    {
        [Required]
        public InvoiceType Type { get; set; }
        [Required]
        [StringLength(4, MinimumLength = 4, ErrorMessage = "PointOfSale must be exactly 4 digits.")]
        public string PointOfSale { get; set; } = string.Empty;
        [Required]
        [StringLength(8, MinimumLength = 8, ErrorMessage = "Number must be exactly 8 digits.")]
        public string Number { get; set; } = string.Empty;
    }
}