using norviguet_control_fletes_api.Entities;
using System.ComponentModel.DataAnnotations;

namespace norviguet_control_fletes_api.Models.Invoice
{
    public class CreateInvoiceDto
    {
        [Required]
        public InvoiceType Type { get; set; } = InvoiceType.A;
        [Required]
        [StringLength(5, MinimumLength = 5, ErrorMessage = "PointOfSale must be exactly 5 digits.")]
        public string PointOfSale { get; set; } = string.Empty;
        [Required]
        [StringLength(8, MinimumLength = 8, ErrorMessage = "Number must be exactly 8 digits.")]
        public string InvoiceNumber { get; set; } = string.Empty;
        [Required]
        public int Price { get; set; }
        [Required]
        public int OrderId { get; set; }
    }
}